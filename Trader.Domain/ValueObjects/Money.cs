namespace Trader.Domain.ValueObjects;

/// <summary>مبلغ پول با ارز. readonly record struct برای Performance در محاسبات.</summary>
public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "IRR")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "IRR") => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public Money Divide(decimal divisor)
    {
        if (divisor == 0m) throw new DivideByZeroException();
        return new Money(Amount / divisor, Currency);
    }

    public bool IsPositive => Amount > 0m;
    public bool IsZero => Amount == 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Currency mismatch: {Currency} vs {other.Currency}");
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}