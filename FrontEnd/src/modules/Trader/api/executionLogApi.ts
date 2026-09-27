import getAPI from "@/core/api/axiosClient";
import type { ExecutionLogQueryResult } from "../models";

const API_MODULE = "trader";

export const executionLogApi = {
  GetLogs: async (params: {
    planId?: string;
    level?: string;
    search?: string;
    skip?: number;
    take?: number;
  }): Promise<ExecutionLogQueryResult> => {
    const api = getAPI(API_MODULE);
    const searchParams = new URLSearchParams();

    if (params.planId) searchParams.set("planId", params.planId);
    if (params.level && params.level !== "all")
      searchParams.set("level", params.level);
    if (params.search) searchParams.set("search", params.search);
    searchParams.set("skip", String(params.skip ?? 0));
    searchParams.set("take", String(params.take ?? 200));

    const response = await api.get<ExecutionLogQueryResult>(
      `/api/Trader/ExecutionLog/GetLogs?${searchParams}`,
      { withCredentials: true },
    );
    return response.data;
  },
};