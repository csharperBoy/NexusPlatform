// src/modules/Trader/api/strategyTuningApi.ts
import getAPI from "@/core/api/axiosClient";
import type {
  CreateStrategyTuningModel,
  StrategyTuningRunSummaryView,
  StrategyTuningRunDetailView,
  StrategyTuningResultView,
  ParameterDefinitionView,
} from "../models";

const API_MODULE = "trader";

export const strategyTuningApi = {

  
  Run: async (model: CreateStrategyTuningModel): Promise<string> => {
    const api = getAPI(API_MODULE);
    const r = await api.post<string>("/api/Trader/StrategyTuning/Run", model,
      { withCredentials: true });
    return r.data;
  },
  Cancel: async (id: string): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const r = await api.post<boolean>(`/api/Trader/StrategyTuning/Cancel/${id}`, {},
      { withCredentials: true });
    return r.data;
  },
  GetList: async (take = 100): Promise<StrategyTuningRunSummaryView[]> => {
    const api = getAPI(API_MODULE);
    const r = await api.get<StrategyTuningRunSummaryView[]>(
      `/api/Trader/StrategyTuning/GetList?take=${take}`, { withCredentials: true });
    return r.data;
  },
  GetById: async (id: string): Promise<StrategyTuningRunDetailView> => {
    const api = getAPI(API_MODULE);
    const r = await api.get<StrategyTuningRunDetailView>(
      `/api/Trader/StrategyTuning/GetById/${id}`, { withCredentials: true });
    return r.data;
  },
  GetResults: async (id: string, topOnly = false): Promise<StrategyTuningResultView[]> => {
    const api = getAPI(API_MODULE);
    const r = await api.get<StrategyTuningResultView[]>(
      `/api/Trader/StrategyTuning/Results/${id}?topOnly=${topOnly}`, { withCredentials: true });
    return r.data;
  },
GetParameterSpace: async (strategyKey: string, primaryTf: number):
  Promise<ParameterDefinitionView[]> => {
  const api = getAPI(API_MODULE);
  const r = await api.get<ParameterDefinitionView[]>(
    `/api/Trader/StrategyTuning/ParameterSpace/${strategyKey}?primaryTimeFrame=${primaryTf}`,
    { withCredentials: true });
  return r.data;
},
};