// src/core/api/axiosClient.ts
import axios, {
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";

import { getAccessToken , setGlobalAccessToken } from "@/modules/Identity/context/AuthContext";
/* ============================================================
   ENV CONFIG
============================================================ */

const CURRENT_PROJECT = import.meta.env.VITE_CURRENT_PROJECT || "default";

/* ============================================================
   DISCOVER API MODULES
============================================================ */

function getAPIModules(): Record<string, string> {
  const modules: Record<string, string> = {};
  const prefix = `VITE_${CURRENT_PROJECT.toUpperCase()}_`;
  
  Object.keys(import.meta.env).forEach((key) => {
    if (key.startsWith(prefix) && key.endsWith("_API")) {
      const moduleName = key
        .slice(prefix.length, -4)
        .toLowerCase();

      modules[moduleName] = import.meta.env[key] as string;
    }
  });

  return modules;
}

const apiModules = getAPIModules();

/* ============================================================
   REFRESH CONTROL
============================================================ */

let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

function subscribeTokenRefresh(cb: (token: string) => void) {
  refreshSubscribers.push(cb);
}

function onRefreshed(token: string) {
  refreshSubscribers.forEach((cb) => cb(token));
  refreshSubscribers = [];
}

/* ============================================================
   CREATE CLIENTS
============================================================ */

type AxiosClients = Record<string, AxiosInstance>;
const axiosClients: AxiosClients = {};

Object.entries(apiModules).forEach(([moduleName, baseURL]) => {
    
  const client = axios.create({
    baseURL,
    timeout: 15000,
    withCredentials: true, // 🔥 required for cookie refresh
    headers: {
      "Content-Type": "application/json",
    },
  });

  /* ===========================
     REQUEST INTERCEPTOR
  =========================== */

  client.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const token = getAccessToken();
      if (token) {
        config.headers.set("Authorization", `Bearer ${token}`);
      }

      return config;
    }
  );

  /* ===========================
     RESPONSE INTERCEPTOR
  =========================== */

  client.interceptors.response.use(
    (response: AxiosResponse) => response,
    async (error) => {
      const originalRequest: any = error.config;

      if (
        error.response?.status === 401 &&
        !originalRequest._retry &&
        !originalRequest.url?.includes("/refresh")
      ) {
        if (isRefreshing) {
          return new Promise((resolve) => {
            subscribeTokenRefresh((token: string) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              resolve(client(originalRequest));
            });
          });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
          const refreshResponse = await axios.post(
            `${baseURL}/api/identity/auth/refresh`,
            {},
            { withCredentials: true }
          );

          const newToken = refreshResponse.data.accessToken;

          setGlobalAccessToken(newToken);
          onRefreshed(newToken);

          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return client(originalRequest);
        } catch (refreshError) {
          setGlobalAccessToken(null);
          window.location.href = "/login";
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      return Promise.reject(error);
    }
  );

  axiosClients[moduleName] = client;
});

/* ============================================================
   GET API
============================================================ */

export function getAPI(moduleName: string): AxiosInstance {
  const client = axiosClients[moduleName];

  if (!client) {
    throw new Error(
      `Axios client for module "${moduleName}" not found.`
    );
  }

  return client;
}

export default getAPI;
