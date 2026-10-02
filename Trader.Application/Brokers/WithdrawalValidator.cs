using Trader.Application.Dtos.Finance;

namespace Trader.Application.Brokers;

/// <summary>
/// اعتبارسنجی درخواست برداشت وجه قبل از ارسال به کارگزاری.
///
/// ⚠️ این کلاس broker-agnostic است و فقط به Application DTOها وابسته‌ست.
/// برای هر کارگزاری جدید، اگه قاعده‌ی محدودیت متفاوت باشه،
/// می‌تونی overload اضافه کنی یا policy جدا بدی.
///
/// ⚠️ نکته‌ی حیاتی: این validator فقط "شکل درخواست" رو چک می‌کنه.
/// Idempotency (جلوگیری از ارسال تکراری) وظیفه‌ی لایه‌ی دیگریه.
/// </summary>
public static class WithdrawalValidator
{
    public sealed record ValidationResult(bool IsValid, string? Reason)
    {
        public static ValidationResult Ok() => new(true, null);
        public static ValidationResult Fail(string reason) => new(false, reason);
    }

    public static ValidationResult Validate(
        PaymentAccountBalancesDto balances,
        WithdrawalRequestDto request)
    {
        /* ─── ۰. مبلغ ─── */
        if (request.AmountRial <= 0)
            return ValidationResult.Fail("Amount must be positive");

        /* ─── ۱. بانک ─── */
        var bank = balances.BankAccounts.FirstOrDefault(b => b.Id == request.BankAccountId);
        if (bank is null)
            return ValidationResult.Fail($"BankAccountId {request.BankAccountId} not found");

        if (!bank.IsActive)
            return ValidationResult.Fail($"Bank account {request.BankAccountId} is inactive");

        /* ─── ۲. IBAN با بانک هم‌خوانی داشته باشه ─── */
        if (!string.Equals(bank.ShebaNumber, request.Iban, StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Fail(
                $"IBAN mismatch: request={request.Iban}, bank={bank.ShebaNumber}");

        /* ─── ۳. محدودیت کلی مشتری ─── */
        if (balances.IsCustomerConstraintRestricted)
            return ValidationResult.Fail("Customer is under restriction");

        /* ─── ۴. اطلاعات تاریخی ─── */
        var dateBal = balances.BalancesPerDate
            .FirstOrDefault(d => d.PerformDate == request.PerformDate);
        if (dateBal is null)
            return ValidationResult.Fail(
                $"No balance info available for {request.PerformDate:yyyy-MM-dd}");

        /* ─── ۵. موجودی کافی ─── */
        if (request.AmountRial > dateBal.AvailableBalanceRial)
            return ValidationResult.Fail(
                $"Amount {request.AmountRial:N0} Rial exceeds available balance " +
                $"{dateBal.AvailableBalanceRial:N0} Rial");

        /* ─── ۶. سقف تک‌درخواست ─── */
        if (request.AmountRial > dateBal.MaxSingleRequestAmountRial)
            return ValidationResult.Fail(
                $"Amount {request.AmountRial:N0} Rial exceeds single-request limit " +
                $"{dateBal.MaxSingleRequestAmountRial:N0} Rial");

        /* ─── ۷. محدودیت per-bank ─── */
        var bankBal = dateBal.PerBank
            .FirstOrDefault(b => b.BankAccountId == request.BankAccountId);
        if (bankBal is null)
            return ValidationResult.Fail(
                $"No per-bank balance for bank {request.BankAccountId} on {request.PerformDate:yyyy-MM-dd}");

        if (!bankBal.IsBankAvailable)
            return ValidationResult.Fail(
                bankBal.RequestRestrictionDetail ?? "Bank unavailable at this time");

        if (request.AmountRial > bankBal.SingleRequestAmountRial)
            return ValidationResult.Fail(
                $"Amount {request.AmountRial:N0} Rial exceeds bank single-request limit " +
                $"{bankBal.SingleRequestAmountRial:N0} Rial");

        return ValidationResult.Ok();
    }

    /// <summary>
    /// پیدا کردن بهترین تاریخ برای برداشت (اولین تاریخی که مبلغ درخواستی مجازه).
    /// مفیده وقتی کاربر می‌خواد "ASAP" برداشت کنه.
    /// </summary>
    public static DateOnly? FindEarliestDateFor(
        PaymentAccountBalancesDto balances,
        long bankAccountId,
        long amountRial)
    {
        return balances.BalancesPerDate
            .Where(d => d.PerBank.Any(pb =>
                pb.BankAccountId == bankAccountId &&
                pb.IsBankAvailable &&
                pb.SingleRequestAmountRial >= amountRial))
            .OrderBy(d => d.PerformDate)
            .Select(d => (DateOnly?)d.PerformDate)
            .FirstOrDefault();
    }
}