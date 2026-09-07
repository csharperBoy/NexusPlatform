// src/core/components/Generic/templates/pages/TestPage.tsx

import React from 'react';
import { CrudProvider } from '../../core/CrudProvider';
import { GenericCrudApi, SelectionListDto } from '../../core/types';
import { TestEntity, TestView } from '../views/TestView';

// دیتای موک جهت تست اولیه
const mockData: TestEntity[] = [
  { id: '1', title: 'کامپوننت جدول پویا', category: 'ویژگی‌های نمایشی' },
  { id: '2', title: 'مدیریت فرم ایجاد/ویرایش', category: 'فرم‌ها' },
  { id: '3', title: 'خروجی فایل اکسل', category: 'گزارش‌گیری' },
  { id: '4', title: 'ورود دسته‌جمعی از اکسل', category: 'گزارش‌گیری' },
  { id: '5', title: 'جستجوی پیشرفته سراسری', category: 'فیلترها' },
  { id: '6', title: 'جستجوی پیشرفته 2', category: 'فیلترها' },
  { id: '7', title: 'جستجوی پیشرفته 3', category: 'فیلترها' },
  { id: '8', title: 'جستجوی پیشرفته 4', category: 'فیلترها' },
  { id: '9', title: 'جستجوی پیشرفته 5', category: 'فیلترها' },
  { id: '10', title: 'جستجوی پیشرفته 6', category: 'فیلترها' },
  { id: '11', title: 'جستجوی پیشرفته 7', category: 'فیلترها' },
  { id: '12', title: 'جستجوی پیشرفته 8', category: 'فیلترها' },

];

// پیاده‌سازی سرویس API ماک منطبق بر اینترفیس GenericCrudApi
const mockApi: GenericCrudApi<
  TestEntity, 
  Omit<TestEntity, 'id'>, 
  TestEntity, 
  { query?: string }
> = {
  getList: async () => {
    await new Promise((resolve) => setTimeout(resolve, 600)); // شبیه‌سازی تاخیر شبکه
    return [...mockData];
  },
  search: async (req) => {
    await new Promise((resolve) => setTimeout(resolve, 600));
    if (!req.query) return mockData;
    return mockData.filter((item) => item.title.includes(req.query!));
  },
  create: async (cmd) => {
    const newItem = { id: String(Date.now()), ...cmd };
    mockData.push(newItem);
    return newItem;
  },
  batchCreate: async (cmds) => {
    const created = cmds.map((cmd, i) => ({ id: String(Date.now() + i), ...cmd }));
    mockData.push(...created);
    return created;
  },
  update: async (cmd) => {
    const index = mockData.findIndex((x) => x.id === cmd.id);
    if (index !== -1) mockData[index] = cmd;
    return cmd;
  },
  batchUpdate: async (cmds) => {
    cmds.forEach((cmd) => {
      const index = mockData.findIndex((x) => x.id === cmd.id);
      if (index !== -1) mockData[index] = cmd;
    });
    return cmds;
  },
  delete: async (id) => {
    const index = mockData.findIndex((x) => x.id === id);
    if (index !== -1) mockData.splice(index, 1);
  },
  batchDelete: async (ids) => {
    ids.forEach((id) => {
      const index = mockData.findIndex((x) => x.id === id);
      if (index !== -1) mockData.splice(index, 1);
    });
  },
  getSelectionList: async (): Promise<SelectionListDto[]> => {
    return mockData.map((item) => ({
      value: item.id,
      label: item.title,
      display: `${item.title} (${item.category})`,
    }));
  },
};

export const TestPage: React.FC = () => {
  return (
    <CrudProvider api={mockApi}>
      <div className="min-h-screen bg-gray-100 py-10">
        <TestPageHeader />
        <TestView />
      </div>
    </CrudProvider>
  );
};

const TestPageHeader: React.FC = () => (
  <div className="max-w-4xl mx-auto mb-6 text-center">
    <h1 className="text-2xl font-black text-gray-900">صفحه تست یکپارچه Generic CRUD</h1>
    <p className="text-gray-600 text-sm mt-1">
      این صفحه برای بررسی عملکرد گام‌به‌گام Sliceها و کامپوننت‌های توسعه‌داده‌شده است.
    </p>
  </div>
);