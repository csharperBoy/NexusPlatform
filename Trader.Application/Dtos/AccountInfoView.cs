namespace Trader.Application.Dtos
{
    public class AccountInfoView
    {
        public Guid Id { get; set; }
        public int Broker { get; set; }
        public string Name { get; set; } = default!;
        public string Username { get; set; } = default!;

        /// <summary>"empty" | "valid" | "expired" | "invalid"</summary>
        public string SessionStatus { get; set; } = "empty";

        public long? SessionExp { get; set; }
    }
}