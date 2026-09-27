export interface ExecutionLogInfoView {
  id: string;
  planId: string;
  planName?: string | null;
  orderId?: string | null;
  symbolIsin?: string | null;
  timestamp: number;    // Unix ms
  level: "info" | "warning" | "error" | "success";
  message: string;
  code?: number | null;
}

export interface ExecutionLogQueryResult {
  items: ExecutionLogInfoView[];
  totalCount: number;
}