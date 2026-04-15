using AzureFunction.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using Moq;

namespace Tests
{
    public class QueryFunctionTests
    {
        private readonly Mock<IEmbeddingClient> _embeddingMock = new();
        private readonly Mock<IAiSearchClient> _searchMock = new();
        private readonly Mock<IChatCompletionClient> _chatMock = new();
        private readonly Mock<ILogger<QueryFunction>> _loggerMock = new();

        private QueryFunction CreateFunction()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

            return new QueryFunction(
                loggerFactory.Object,
                _embeddingMock.Object,
                _searchMock.Object,
                _chatMock.Object);
        }

        // missing query tests
        [Fact]
        public async Task Run_NoQuery_ReturnsBadRequest()
        {
            var function = CreateFunction();
            var request = new TestHttpRequestData(new Uri("http://localhost/api/query"), method: "GET");
            var context = new TestFunctionContext();

            var response = await function.Run(request, context);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Run_EmptyQuery_ReturnsBadRequest(string value)
        {
            var function = CreateFunction();

            var uri = new Uri($"http://localhost/api/query?q={value}");
            var request = new TestHttpRequestData(uri, method: "GET");
            var context = new TestFunctionContext();

            var response = await function.Run(request, context);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Happy path tests
        [Fact]
        public async Task Run_ValidRequest_ReturnsOk_AndCallsDependencies()
        {
            var function = CreateFunction();
            var question = "test question";

            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new float[] { 1, 2 });

            _searchMock
                .Setup(s => s.SearchByVectorAsync(
                    It.IsAny<float[]>(),
                    5,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SearchDocumentChunk>
                {
                new() { Content = "chunk1" },
                new() { Content = "chunk2" }
                });

            _chatMock
                .Setup(c => c.GetAnswerAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("final answer");

            var request = new TestHttpRequestData(
                new Uri($"http://localhost/api/query?q={question}"), method: "GET");
            var context = new TestFunctionContext();

            var response = await function.Run(request, context);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _embeddingMock.Verify(e => e.GetEmbeddingAsync(question, It.IsAny<CancellationToken>()), Times.Once);
            _searchMock.Verify(s => s.SearchByVectorAsync(
                It.IsAny<float[]>(),
                5,
                It.IsAny<CancellationToken>()),
                Times.Once);

            _chatMock.Verify(c => c.GetAnswerAsync(
                "You are a helpful assistant.",
                It.Is<string>(p =>
                    p.Contains("chunk1") &&
                    p.Contains("chunk2") &&
                    p.Contains(question)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Dependency failure tests
        [Fact]
        public async Task Run_EmbeddingThrows_Returns500()
        {
            var function = CreateFunction();
            var ct = CancellationToken.None;

            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("boom"));

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/query?q=test"), method: "GET");

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        // Cancellation propagation tests
        [Fact]
        public async Task Run_WhenCancelled_Rethrows()
        {
            var function = CreateFunction();

            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/query?q=test"), method: "GET");

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => function.Run(request, new TestFunctionContext()));
        }

        // Promt builder unit tests
        [Fact]
        public void BuildPrompt_IncludesChunksAndQuestion()
        {
            var function = CreateFunction();

            var prompt = function.BuildPrompt(
                "my question",
                new[]
                {
                new SearchDocumentChunk { Content = "A" },
                new SearchDocumentChunk { Content = "B" }
                });

            Assert.Contains("A", prompt);
            Assert.Contains("B", prompt);
            Assert.Contains("my question", prompt);
        }

    }
}