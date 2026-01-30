using Google.Protobuf.WellKnownTypes;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;

internal sealed class TestHttpCookies : HttpCookies
{
    private readonly List<IHttpCookie> _cookies = new();

    public override void Append(string name, string value)
    {
        _cookies.Add(new TestHttpCookie(name, value));
    }

    public override void Append(IHttpCookie cookie)
    {
        _cookies.Add(cookie);
    }

    public override IHttpCookie CreateNew()
    {
        return new HttpCookie("new", "none");
    }

    // Minimal stub implementation of IHttpCookie
    private class TestHttpCookie : IHttpCookie
    {
        public TestHttpCookie(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; set; }
        public string? Domain { get; set; }

        public DateTimeOffset? Expires { get; set; }

        public bool? HttpOnly { get; set; }

        public double? MaxAge { get; set; }

        public string? Path { get; set; }

        public SameSite SameSite { get; set; }

        public bool? Secure { get; set; }
    }
}