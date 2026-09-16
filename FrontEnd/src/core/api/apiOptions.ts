//src/core/api/apiOptions.ts
export type OfflineStrategy = 'direct' | 'queueOffline' | 'cacheFirst';

// تایپ برای آپشن‌های کاستوم متدهای API
export interface ApiOptions {
  offlineStrategy?: OfflineStrategy;
  // می‌تونی آپشن‌های دیگه رو هم اینجا اضافه کنی
}

// مقدار پیش‌فرض رو در یک ثابت نگهداری می‌کنیم
export const DEFAULT_OPTIONS: ApiOptions = {
  offlineStrategy: 'direct',
};