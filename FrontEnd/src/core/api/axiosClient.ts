// src/core/api/axiosClient.ts
import axios, {
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { storageAdapter } from "@/core/storage/storageAdapter";
import { offlineQueue } from "./offlineQueue";
import { OfflineStrategy } from "./apiOptions";

/* ============================================================
   AXIOS TYPE EXTENSION
============================================================ */
declare module "axios" {
  export interface AxiosRequestConfig {
    offlineStrategy?: OfflineStrategy;
    moduleName?: string;
  }
}

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
   CACHE HELPERS
============================================================ */

// ساخت کلید یکتا برای کش کردن هر درخواست بر اساس URL و پارامترها
function generateCacheKey(config: InternalAxiosRequestConfig | any): string {
  const url = config.url || "";
  const params = config.params ? JSON.stringify(config.params) : "";
  return `req_cache_${url}_${params}`;
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
    async (config: InternalAxiosRequestConfig) => {
      const token = await storageAdapter.getAccessToken();
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
    async (response: AxiosResponse) => {
      // 🟢 بخش جدید: ذخیره دیتا در صورت موفقیت‌آمیز بودن درخواست GET
      const config = response.config;
      if (config.method?.toUpperCase() === "GET") {
        const cacheKey = generateCacheKey(config);
        // به صورت Async ذخیره می‌کنیم تا جلوی رندر شدن UI را نگیرد
        storageAdapter.setItem(cacheKey, JSON.stringify(response.data)).catch(console.error);
      }
      return response;
    },
    async (error) => {
      const originalRequest: any = error.config;
      const method = originalRequest?.method?.toUpperCase();
      
      // 🔴 هندل کردن حالت آفلاین (Network Error یا Timeout)
    if (!error.response) {
      // حالت اول: خواندن GET از کش
      if (method === "GET") {
        try {
          const cacheKey = generateCacheKey(originalRequest);
          const cachedDataStr = await storageAdapter.getItem(cacheKey);
          if (cachedDataStr) {
            return Promise.resolve({
              data: JSON.parse(cachedDataStr),
              status: 200,
              statusText: "OK (Cached)",
              headers: {},
              config: originalRequest,
              request: {},
            } as AxiosResponse);
          }
        } catch (cacheError) {
          console.error("خطا در خواندن از کش", cacheError);
        }
      } 
      
      // حالت صف آفلاین با معماری جدید
      else if (["POST", "PUT", "DELETE"].includes(method)) {
        const strategy = originalRequest.offlineStrategy || "direct";

        if (strategy === "queueOffline") {
          await offlineQueue.enqueue({
            moduleName: originalRequest.moduleName || "default",
            url: originalRequest.url,
            method: method as any,
            data: originalRequest.data ? JSON.parse(originalRequest.data) : undefined,
          });

          // بازگرداندن پاسخ موفق فرضی با پرچم مخصوص جهت عدم کرش کامپوننت و اعمال آپدیت لوکال
          return Promise.resolve({
            data: { _isOfflineQueued: true },
            status: 202,
            statusText: "Accepted (Queued Offline)",
            headers: {},
            config: originalRequest,
            request: {},
          } as AxiosResponse);
        }
      }
    }

      // منطق رفرش توکن (401 Unauthorized)
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

          await storageAdapter.setAccessToken(newToken);
          onRefreshed(newToken);

          originalRequest.headers.Authorization = `Bearer ${newToken}`;
          return client(originalRequest);
        } catch (refreshError) {
          await storageAdapter.removeAccessToken();
          window.dispatchEvent(new CustomEvent('auth:unauthorized'));
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
  const normalized = moduleName.toLowerCase();

  const client = axiosClients[normalized];

  if (!client) {
    console.log("axiosClients keys =", Object.keys(axiosClients));
    console.log("requested =", moduleName, "normalized =", normalized);

    throw new Error(
      `Axios client for module "${normalized}" not found.`
    );
  }

  return client;
}

export default getAPI;