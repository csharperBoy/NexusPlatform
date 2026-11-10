using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Trader.ExternalApiClient.Tgju;
using Xunit;

namespace Trader.Tests.Infrastructure.Tgju;

public class TgjuApiClientTests
{
    private static TgjuApiClient CreateClient(HttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.tgju.org") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Tgju")).Returns(http);

        var options = Options.Create(new TgjuOptions
        {
            BaseUrl = "https://api.tgju.org",
            TimeoutSeconds = 10,
        });

        return new TgjuApiClient(factory.Object, options, Mock.Of<ILogger<TgjuApiClient>>());
    }

    [Fact]
    public async Task FetchPageAsync_ParsesDataTablesResponse()
    {
        const string json = """
        {
          "draw": "1",
          "recordsTotal": 2,
          "recordsFiltered": 2,
          "data": [
            ["2,730,050","2,712,600","2,735,000","2,715,000","<s/>","-","2026/11/02","1405/08/11"],
            ["2,714,850","2,705,000","2,725,000","2,715,000","<s/>","-","2026/11/01","1405/08/10"]
          ]
        }
        """;

        var client = CreateClient(new FakeHandler(json));
        var wire = await client.FetchPageAsync("price_dollar_rl", 0, 30);

        wire.Data.Should().HaveCount(2);
        wire.Data[0][0].Should().Be("2,730,050");
        wire.Data[0][6].Should().Be("2026/11/02");
    }

    [Fact]
    public async Task FetchPageAsync_NonSuccess_ThrowsTgjuException()
    {
        var client = CreateClient(new FakeHandler("err", HttpStatusCode.InternalServerError));

        var act = () => client.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>();
    }

    [Fact]
    public async Task FetchPageAsync_EmptyBody_Throws()
    {
        var client = CreateClient(new FakeHandler(""));

        var act = () => client.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>();
    }

    [Fact]
    public async Task FetchPageAsync_InvalidJson_ThrowsTgjuException()
    {
        var client = CreateClient(new FakeHandler("not-json-at-all"));

        var act = () => client.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>();
    }

    [Theory]
    [InlineData("", 0, 30)]
    [InlineData("x", -1, 30)]
    [InlineData("x", 0, 0)]
    public async Task FetchPageAsync_InvalidArguments_Throws(
        string indicator, int start, int length)
    {
        var client = CreateClient(new FakeHandler("{}"));

        var act = () => client.FetchPageAsync(indicator, start, length);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    /* ═══════════════════ Fake Handler ═══════════════════ */

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public FakeHandler(string body, HttpStatusCode status = HttpStatusCode.OK)
        {
            _body = body;
            _status = status;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            var res = new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(res);
        }
    }
}