namespace Trader.Application.Dtos
{
    public class AccountInfoView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Username { get; set; } = default!;

        /// <summary>"empty" | "valid" | "expired" | "invalid"</summary>
        public string TokenStatus { get; set; } = "empty";

        public string? TokenExp { get; set; }   // ISO 8601
    }
}