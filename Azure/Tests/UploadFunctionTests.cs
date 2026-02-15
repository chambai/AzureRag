using AzureFunction.Client;
//using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using Xunit;


namespace Tests
{
    public class UploadFunctionTests
    {
        private readonly Mock<IBlobStorage> _blobMock = new();
        private readonly Mock<IEmbeddingClient> _embeddingMock = new();
        private readonly Mock<IAiSearchClient> _searchMock = new();
        private readonly Mock<ILogger<UploadFunction>> _loggerMock = new();

        private UploadFunction CreateFunction()
        {
            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(_loggerMock.Object);

            return new UploadFunction(
                loggerFactory.Object,
                _blobMock.Object,
                _embeddingMock.Object,
                _searchMock.Object);
        }

        // missing heaser
        [Fact]
        public async Task Run_MissingFilenameHeader_ReturnsBadRequest()
        {
            var function = CreateFunction();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                body: "data");

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // empty filename
        [Fact]
        public async Task Run_EmptyFilename_ReturnsBadRequest()
        {
            var function = CreateFunction();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                body: "data");

            request.Headers.Add("X-Filename", "   ");

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Empty body
        [Fact]
        public async Task Run_EmptyBody_ReturnsBadRequest()
        {
            var function = CreateFunction();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                body: "");

            request.Headers.Add("X-Filename", "file.txt");

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // no multipart file present
        [Fact]
        public async Task Run_NoMultipartFile_ReturnsBadRequest()
        {
            var function = CreateFunction();

            using var multipart = new MultipartFormDataContent();
            var bodyStream = await multipart.ReadAsStreamAsync();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                bodyStream: bodyStream);

            request.Headers.Add("X-Filename", "file.txt");
            request.Headers.Add("Content-Type", multipart.Headers.ContentType!.ToString());

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // happy path tests
        [Fact]
        public async Task Run_WithValidFile_ProcessesAllChunks()
        {
            var function = CreateFunction();

            var content = "Chunk1 Chunk2 Chunk3";

            using var multipart = new MultipartFormDataContent();
            multipart.Add(
                new StringContent(content),
                "file",
                "file.txt");

            var bodyStream = await multipart.ReadAsStreamAsync();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                bodyStream: bodyStream);

            request.Headers.Add("X-Filename", "file.txt");
            request.Headers.Add("Content-Type", multipart.Headers.ContentType!.ToString());

            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new float[] { 1, 2 });

            _searchMock
                .Setup(s => s.StoreVectorAsync(
                    It.IsAny<float[]>(),
                    It.IsAny<string>(),
                    "file.txt",
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _blobMock.Verify(b =>
                b.UploadAsync("documents", "file.txt", It.IsAny<Stream>()),
                Times.Once);

            _embeddingMock.Verify(e =>
                e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            _searchMock.Verify(s =>
                s.StoreVectorAsync(
                    It.IsAny<float[]>(),
                    It.IsAny<string>(),
                    "file.txt",
                    It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        // Dependency failure tests
        // Blob failure - 500
        [Fact]
        public async Task Run_WhenBlobUploadFails_Returns500()
        {
            var function = CreateFunction();

            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent("text"), "file", "file.txt");

            var bodyStream = await multipart.ReadAsStreamAsync();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                bodyStream: bodyStream);

            request.Headers.Add("X-Filename", "file.txt");
            request.Headers.Add("Content-Type", multipart.Headers.ContentType!.ToString());

            _blobMock
                .Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new Exception("boom"));

            var response = await function.Run(request, new TestFunctionContext());

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        // Cancellation test
        [Fact]
        public async Task Run_WhenCancelled_Rethrows()
        {
            var function = CreateFunction();

            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent("text"), "file", "file.txt");

            var bodyStream = await multipart.ReadAsStreamAsync();

            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                bodyStream: bodyStream);

            request.Headers.Add("X-Filename", "file.txt");
            request.Headers.Add("Content-Type", multipart.Headers.ContentType!.ToString());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            var context = new TestFunctionContext(cts.Token);

            // Include CancellationToken in the setup
            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            await Assert.ThrowsAsync<OperationCanceledException>(
                () => function.Run(request, context));
        }

        // Validate request
        [Fact]
        public void ValidateRequest_Valid_ReturnsTrue()
        {
            var function = CreateFunction();

            var request = new TestHttpRequestData(
                new Uri("http://localhost"),
                method: "POST",
                body: "data");

            request.Headers.Add("X-Filename", "file.txt");

            var result = function.ValidateRequest(request);

            Assert.True(result.IsValid);
            Assert.Equal("file.txt", result.FileName);
        }

        // Calls per chunk
        [Fact]
        public async Task ProcessChunksAsync_CallsEmbeddingAndStorePerChunk()
        {
            var function = CreateFunction();

            var chunks = new[] { "A", "B", "C" };

            _embeddingMock
                .Setup(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new float[] { 1 });

            await function.ProcessChunksAsync(
                chunks,
                "file.txt",
                CancellationToken.None);

            _embeddingMock.Verify(e => e.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

            _searchMock.Verify(s =>
                s.StoreVectorAsync(
                    It.IsAny<float[]>(),
                    It.IsAny<string>(),
                    "file.txt",
                    It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }
    }
}
