// core/api/axiosClient.ts
import axios from "axios";

const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "https://localhost:5001/api",
  headers: {
    "Content-Type": "application/json",
  },
});

// اضافه کردن توکن به هر درخواست
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// هندل ارورها
axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.warn("Unauthorized! Redirect to login...");
    }
    return Promise.reject(error);
  }
);

export default axiosClient;
