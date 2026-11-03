namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// انتخاب Dispatcher بر اساس Mode اجرا (Live یا Paper).
/// </summary>
public interface IOrderDispatcherResolver
{
    IBrokerOrderDispatcher Resolve(bool isPaper);
}