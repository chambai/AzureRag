using HttpMultipartParser;
using Microsoft.Extensions.Logging;
using System.Net;
using Tests.Fakes;

namespace Tests
{
    public class UploadFunctionTests
    {
        private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

        [Fact]
        public async Task UploadFunction_Run_NoFile_ReturnsBadRequest()
        {
            var request = new TestHttpRequestData(
            new Uri("http://localhost/api/upload"),
            method: "POST",
            body: ""
            );

            request.Headers.Add("X-Filename", "Docs/teacher_contract.txt");
            var context = new TestFunctionContext();

            var function = new UploadFunction(_loggerFactory, new FakeBlobStorage(), new FakeEmbeddingClient(), new FakeSearchClient());


            var response = await function.Run(request, context);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UploadFunction_Run_WithFile_ReturnsOk()
        {
            // Arrange
            //var filePath = Path.Combine(
            //AppContext.BaseDirectory,
            //"Docs",
            //"teacher_contract.txt");

            //var fileContent = await File.ReadAllTextAsync(filePath);

            //var request = new TestHttpRequestData(
            //    new Uri("http://localhost/api/upload"),
            //    method: "POST",
            //    body: fileContent
            //);

            //request.Headers.Add("X-Filename", "Docs/teacher_contract.txt");

            var filePath = "Docs/teacher_contract.txt";

            // Load the actual file bytes
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            // Create the Multipart wrapper
            using var multipartContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);

            // This MUST match the key name you use in the parser: parser.Files.GetFile("file")
            multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));

            // Get the boundary-encoded stream and headers
            var bodyStream = await multipartContent.ReadAsStreamAsync();
            var contentType = multipartContent.Headers.ContentType.ToString();

            // 4. Create your test request
            var request = new TestHttpRequestData(
                new Uri("http://localhost/api/upload"),
                method: "POST",
                bodyStream: bodyStream
            );

            request.Headers.Add("X-Filename", "Docs/teacher_contract.txt");

            // set the Content-Type header so the parser finds the boundary
            request.Headers.Add("Content-Type", contentType);

            var context = new TestFunctionContext();

            var function = new UploadFunction(_loggerFactory, new FakeBlobStorage(), new FakeEmbeddingClient(), new FakeSearchClient());

            // Act
            var response = await function.Run(request, context);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            Assert.Contains("Document uploaded", body);
        }
    }
}