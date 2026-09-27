namespace Trader.Application.Dtos
{
    public class ExecutionLogInfoView
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string? PlanName { get; set; }
        public Guid? OrderId { get; set; }
        public string? SymbolIsin { get; set; }

        /// <summary>Unix ms</summary>
        public long Timestamp { get; set; }

        public string Level { get; set; } = "info";
        public string Message { get; set; } = default!;
        public int? Code { get; set; }
    }

    public class ExecutionLogQueryResult
    {
        public List<ExecutionLogInfoView> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}