using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Domain.Enums;
using Trader.Infrastructure.PriceCollection;
using Xunit;

namespace Trader.Tests.Infrastructure.PriceCollection;

public class PriceSourceProviderResolverTests
{
    [Fact]
    public void Resolve_ReturnsRegisteredProvider()
    {
        var tgju = Mock.Of<IPriceSourceProvider>(p => p.SourceType == PriceSourceType.Tgju);

        var resolver = new PriceSourceProviderResolver(
            new[] { tgju }, Mock.Of<ILogger<PriceSourceProviderResolver>>());

        resolver.Resolve(PriceSourceType.Tgju).Should().BeSameAs(tgju);
    }

    [Fact]
    public void Resolve_ReturnsNullForUnregisteredType()
    {
        var tgju = Mock.Of<IPriceSourceProvider>(p => p.SourceType == PriceSourceType.Tgju);

        var resolver = new PriceSourceProviderResolver(
            new[] { tgju }, Mock.Of<ILogger<PriceSourceProviderResolver>>());

        resolver.Resolve(PriceSourceType.TelegramChannel).Should().BeNull();
    }

    [Fact]
    public void Constructor_RejectsDuplicateSourceType()
    {
        var a = Mock.Of<IPriceSourceProvider>(p => p.SourceType == PriceSourceType.Tgju);
        var b = Mock.Of<IPriceSourceProvider>(p => p.SourceType == PriceSourceType.Tgju);

        var act = () => new PriceSourceProviderResolver(
            new[] { a, b }, Mock.Of<ILogger<PriceSourceProviderResolver>>());

        act.Should().Throw<ArgumentException>();
    }
}