import getAPI from "@/core/api/axiosClient";
import type {
  KillSwitchStatusView,
  LossStreakState,
  ActivateKillSwitchCommand,
  DeactivateKillSwitchCommand,
  ResetCircuitBreakerCommand,
} from "../models";

const API_MODULE = "trader";

/**
 * Risk Dashboard API client.
 *
 * Response contracts (از Backend HandleResult unwrap):
 *   • KillSwitchActivate / Deactivate → boolean
 *   • CircuitBreakerReset             → number (تعداد reset شده)
 *   • KillSwitchStatus                → KillSwitchStatusView
 *   • CircuitBreakerActive            → LossStreakState[] (مستقیم از Ok())
 *
 * Failure (4xx/5xx) → axios throws. Caller باید try/catch کند.
 */
export const riskApi = {
  KillSwitchStatus: async (): Promise<KillSwitchStatusView> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<KillSwitchStatusView>(
      "/api/Trader/Risk/Kill-Switch/Status",
      { withCredentials: true },
    );
    return response.data;
  },

  KillSwitchActivate: async (
    cmd: ActivateKillSwitchCommand,
  ): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/Risk/Kill-Switch/Activate",
      cmd,
      { withCredentials: true },
    );
    return response.data;
  },

  KillSwitchDeactivate: async (
    cmd: DeactivateKillSwitchCommand,
  ): Promise<boolean> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/Risk/Kill-Switch/Deactivate",
      cmd,
      { withCredentials: true },
    );
    return response.data;
  },

  CircuitBreakerReset: async (
    cmd: ResetCircuitBreakerCommand,
  ): Promise<number> => {
    const api = getAPI(API_MODULE);
    const response = await api.post<number>(
      "/api/Trader/Risk/CircuitBreaker/Reset",
      cmd,
      { withCredentials: true },
    );
    return response.data;
  },

  CircuitBreakerActive: async (): Promise<LossStreakState[]> => {
    const api = getAPI(API_MODULE);
    const response = await api.get<LossStreakState[]>(
      "/api/Trader/Risk/CircuitBreaker/Active",
      { withCredentials: true },
    );
    return response.data;
  },
};