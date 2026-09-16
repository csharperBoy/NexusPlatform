// src/core/storage/storageAdapter.ts
import { Capacitor } from '@capacitor/core';
import { Preferences } from '@capacitor/preferences';

const TOKEN_KEY = 'access_token';

export const storageAdapter = {
  // متد عمومی برای خواندن
  async getItem(key: string): Promise<string | null> {
    if (Capacitor.isNativePlatform()) {
      const { value } = await Preferences.get({ key });
      return value;
    }
    return Promise.resolve(localStorage.getItem(key));
  },

  // متد عمومی برای نوشتن
  async setItem(key: string, value: string): Promise<void> {
    if (Capacitor.isNativePlatform()) {
      await Preferences.set({ key, value });
    } else {
      localStorage.setItem(key, value);
    }
  },

  // متد عمومی برای حذف
  async removeItem(key: string): Promise<void> {
    if (Capacitor.isNativePlatform()) {
      await Preferences.remove({ key });
    } else {
      localStorage.removeItem(key);
    }
  },

  // === متدهای کمکی مخصوص توکن ===
  async getAccessToken(): Promise<string | null> {
    return this.getItem(TOKEN_KEY);
  },

  async setAccessToken(token: string): Promise<void> {
    return this.setItem(TOKEN_KEY, token);
  },

  async removeAccessToken(): Promise<void> {
    return this.removeItem(TOKEN_KEY);
  }
};