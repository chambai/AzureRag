using AzureFunction.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using Tests.Fakes;

namespace Tests
{
    public class QueryFunctionTests
    {
        private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

        [Fact]
        public async Task QueryFunction_Run_Returns_Text()
        {
            var qValue = "How much termination notice must teachers give";
            var uriBuilder = new UriBuilder("http://localhost/api/query");
            uriBuilder.Query = $"q={Uri.EscapeDataString(qValue)}";

            // Arrange
            var request = new TestHttpRequestData(uriBuilder.Uri, method: "GET");

            var function = new QueryFunction(
                _loggerFactory,
                new FakeEmbeddingClient(),
                new FakeSearchClient(),
                new FakeChatCompletionClient());

            var context = new TestFunctionContext();

            // Act
            var response = await function.Run(request, context);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine(body);
            Assert.Matches(@".+", body); // at least 1 character
        }

        [Fact]
        public async Task QueryFunction_Run_NoQueryParameter_ReturnsBadRequest()
        {
            // Arrange
            var request = new TestHttpRequestData(new Uri("http://localhost/api/query"), method: "GET");
            // no query parameter "q" added to request.Query

            var function = new QueryFunction(
                _loggerFactory,
                new FakeEmbeddingClient(),
                new FakeSearchClient(),
                new FakeChatCompletionClient());

            var context = new TestFunctionContext();

            // Act
            var response = await function.Run(request, context);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // in the isolated worker model, the response body is a stream - read it manually
            // go back to the begining of the stream as the stream position is at the end after writing the response
            // if you don't do this, an empty string is returned
            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body);
            var body = await reader.ReadToEndAsync();
            Assert.Contains("Missing query parameter 'q'.", body);
        }
    }
}