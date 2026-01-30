using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Security.Claims;
using System.Text;

internal sealed class TestHttpRequestData : HttpRequestData
{
    public TestHttpRequestData(Uri url, string method)
        : base(new TestFunctionContext())
    {
        Url = url;
        Method = method;
        Body = new MemoryStream();
        Headers = new HttpHeadersCollection();
    }

    public TestHttpRequestData(Uri url, string method, string body)
        : base(new TestFunctionContext())
    {
        Url = url;
        Method = method;
        Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        Headers = new HttpHeadersCollection();
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies
        => Array.Empty<IHttpCookie>();
    public override Uri Url { get; }
    public override string Method { get; }

    public override IEnumerable<ClaimsIdentity> Identities => throw new NotImplementedException();

    public override HttpResponseData CreateResponse()
    {
        // Pass a default status code (override after creation)
        return new TestHttpResponseData(FunctionContext, HttpStatusCode.OK);
    }
}