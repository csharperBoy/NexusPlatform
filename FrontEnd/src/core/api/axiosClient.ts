// src/core/api/axiosClient.ts
import axios, { type AxiosInstance, type AxiosResponse } from "axios";

// اسم پروژه جاری (AdminPanel, WebSite, ...) را از env بگیریم یا fallback
const CURRENT_PROJECT = import.meta.env.VITE_CURRENT_PROJECT || "default";

// محیط جاری
const ENV = import.meta.env.MODE; // "development" یا "production"

// پیدا کردن تمام envهای مربوط به API
function getAPIModules(): Record<string, string> {
  const modules: Record<string, string> = {};
  Object.keys(import.meta.env).forEach((key) => {
    // فقط env هایی که مربوط به پروژه جاری و _API هستند
    // قالب: VITE_<PROJECT>_<MODULE>_API
    const prefix = `VITE_${CURRENT_PROJECT.toUpperCase()}_`;
    if (key.startsWith(prefix) && key.endsWith("_API")) {
      const moduleName = key.slice(prefix.length, -4).toLowerCase(); // حذف پیشوند و _API
      modules[moduleName] = import.meta.env[key] as string;
    }
  });
  return modules;
}

// ساخت axios instances
const apiModules = getAPIModules();
type AxiosClients = Record<string, AxiosInstance>;
const axiosClients: AxiosClients = {};

Object.entries(apiModules).forEach(([moduleName, baseURL]) => {
  const client = axios.create({
    baseURL,
    headers: {
      "Content-Type": "application/json",
    },
    timeout: 10000,
  });

  // اضافه کردن توکن به همه درخواست‌ها
  client.interceptors.request.use((config) => {
    const token = localStorage.getItem(`${CURRENT_PROJECT}_token`);
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  // هندل ارورها
  client.interceptors.response.use(
    (response: AxiosResponse) => response,
    (error) => {
      if (error.response?.status === 401) {
        console.warn(`Unauthorized request in module "${moduleName}" for project "${CURRENT_PROJECT}"`);
        // می‌توانید اینجا redirect یا logout خودکار بگذارید
        // localStorage.removeItem(`${CURRENT_PROJECT}_token`);
        // window.location.href = '/login';
      }
      return Promise.reject(error);
    }
  );

  axiosClients[moduleName] = client;
});

// تابع کمکی برای گرفتن یک ماژول مشخص
export function getAPI(moduleName: string): AxiosInstance {
  const client = axiosClients[moduleName];
  if (!client) {
    throw new Error(`Axios client for module "${moduleName}" not found in project "${CURRENT_PROJECT}". Available modules: ${Object.keys(axiosClients).join(', ')}`);
  }
  return client;
}

// export پیش‌فرض برای backward compatibility
export default getAPI;