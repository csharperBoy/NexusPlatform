// src/core/api/offlineQueue.ts
import { storageAdapter } from "@/core/storage/storageAdapter";
import getAPI from "./axiosClient";

export interface QueuedMutation {
  id: string;
  moduleName: string;
  url: string;
  method: "POST" | "PUT" | "DELETE";
  data?: any;
  timestamp: number;
}

const QUEUE_STORAGE_KEY = "offline_mutation_queue";

export const offlineQueue = {
  // دریافت صف کامندهای ذخیره شده
  getQueue: async (): Promise<QueuedMutation[]> => {
    const raw = await storageAdapter.getItem(QUEUE_STORAGE_KEY);
    return raw ? JSON.parse(raw) : [];
  },

  // افزودن کامند جدید به صف
  enqueue: async (mutation: Omit<QueuedMutation, "id" | "timestamp">) => {
    const queue = await offlineQueue.getQueue();
    const newItem: QueuedMutation = {
      ...mutation,
      id: `mut_${Date.now()}_${Math.random().toString(36).substring(2, 7)}`,
      timestamp: Date.now(),
    };
    queue.push(newItem);
    await storageAdapter.setItem(QUEUE_STORAGE_KEY, JSON.stringify(queue));
    console.warn(`[Offline Queue] کامند ذخیره شد: ${mutation.method} ${mutation.url}`);
    return newItem;
  },

  // اجرای کامندهای معوقه پس از وصل شدن اینترنت
  processQueue: async () => {
    const queue = await offlineQueue.getQueue();
    if (queue.length === 0) return;

    console.log(`[Offline Queue] در حال همگام‌سازی ${queue.length} کامند معوقه...`);
    const remainingQueue: QueuedMutation[] = [];

    for (const item of queue) {
      try {
        const api = getAPI(item.moduleName);
        await api.request({
          url: item.url,
          method: item.method,
          data: item.data,
          withCredentials: true,
        });
        console.log(`[Offline Queue] کامند با موفقیت ارسال شد: ${item.url}`);
      } catch (error) {
        console.error(`[Offline Queue] خطا در ارسال کامند معوقه: ${item.url}`, error);
        // اگر خطای شبکه بود نگه‌دار، اگر خطای 4xx/5xx سمت سرور بود حذف کن
        if (!(error as any)?.response) {
          remainingQueue.push(item);
        }
      }
    }

    await storageAdapter.setItem(QUEUE_STORAGE_KEY, JSON.stringify(remainingQueue));
  },
};

// گوش به زنگ بودن برای وصل شدن مجدد اینترنت
window.addEventListener("online", () => {
  offlineQueue.processQueue();
});