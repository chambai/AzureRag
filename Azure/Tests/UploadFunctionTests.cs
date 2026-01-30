using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Net;
using Tests.Fakes;

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

        var function = new UploadFunction(_loggerFactory, new FakeBlobStorage(), new FakeEmbeddingClient());

        var response = await function.Run(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadFunction_Run_WithFile_ReturnsOk()
    {
        // Arrange
        var filePath = Path.Combine(
        AppContext.BaseDirectory,
        "Docs",
        "teacher_contract.txt");

        var fileContent = await File.ReadAllTextAsync(filePath);

        var request = new TestHttpRequestData(
            new Uri("http://localhost/api/upload"),
            method: "POST",
            body: fileContent
        );

        request.Headers.Add("X-Filename", "Docs/teacher_contract.txt");

        var function = new UploadFunction(_loggerFactory, new FakeBlobStorage(), new FakeEmbeddingClient());

        // Act
        var response = await function.Run(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(response.Body).ReadToEndAsync();
        Assert.Contains("Document uploaded", body);
    }
}