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
    // ═══════════════════════════════════════════════════════════════
    // Parse — response valid
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task FetchPageAsync_ValidResponse_ParsesData()
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
        var (sut, _) = CreateSut(json);

        var wire = await sut.FetchPageAsync("price_dollar_rl", 0, 30);

        wire.Data.Should().HaveCount(2);
        wire.Data[0][0].Should().Be("2,730,050");
        wire.Data[0][6].Should().Be("2026/11/02");
        wire.RecordsTotal.Should().Be(2);
    }

    // ═══════════════════════════════════════════════════════════════
    // Error paths
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task FetchPageAsync_NonSuccessStatus_ThrowsTgjuException()
    {
        var (sut, _) = CreateSut("err", HttpStatusCode.InternalServerError);

        var act = () => sut.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>()
            .WithMessage("*500*");
    }

    [Fact]
    public async Task FetchPageAsync_EmptyBody_ThrowsTgjuException()
    {
        var (sut, _) = CreateSut("");

        var act = () => sut.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public async Task FetchPageAsync_InvalidJson_ThrowsTgjuException()
    {
        var (sut, _) = CreateSut("not-json-at-all");

        var act = () => sut.FetchPageAsync("x", 0, 30);

        await act.Should().ThrowAsync<TgjuException>()
            .WithMessage("*parse failed*");
    }

    // ═══════════════════════════════════════════════════════════════
    // Argument validation
    // ═══════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("", 0, 30)]
    [InlineData("  ", 0, 30)]
    [InlineData("x", -1, 30)]
    [InlineData("x", 0, 0)]
    public async Task FetchPageAsync_InvalidArguments_ThrowsArgumentException(
        string indicator, int start, int length)
    {
        var (sut, _) = CreateSut("{}");

        var act = () => sut.FetchPageAsync(indicator, start, length);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static (TgjuApiClient Sut, FakeHttpHandler Handler) CreateSut(
        string body = "{}", HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHttpHandler(body, status);
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.tgju.org"),
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("Tgju")).Returns(http);

        var options = Options.Create(new TgjuOptions
        {
            BaseUrl = "https://api.tgju.org",
            TimeoutSeconds = 10,
        });

        var sut = new TgjuApiClient(
            factory.Object, options, Mock.Of<ILogger<TgjuApiClient>>());

        return (sut, handler);
    }

    // ─── Nested fake (per project convention: FakeGuard/ThrowingGuard) ───

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly string _body;
        private readonly HttpStatusCode _status;

        public FakeHttpHandler(string body, HttpStatusCode status)
        {
            _body = body;
            _status = status;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var res = new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(res);
        }
    }
}