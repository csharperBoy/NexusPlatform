import getAPI from "@/core/api/axiosClient";
import type {
  AccountInfoView,
  CreateAccountCommand,
  UpdateAccountCommand,
} from "../models";
import { SelectionListDto } from "@/core/models/SelectionListDto";
import {
  mockLoad,
  mockSave,
  mockDelay,
  mockUid,
} from "./_mockStorage";

const API_MODULE = "trader";
const USE_MOCK = import.meta.env.VITE_TRADER_USE_MOCK === "true";
const MOCK_KEY = "mock:trader:accounts";

interface MockAccount {
  id: string;
  name: string;
  username: string;
  password: string;
  token: string;
  tokenExp: number | null;
}

function toView(a: MockAccount): AccountInfoView {
  let tokenStatus: AccountInfoView["tokenStatus"] = "empty";
  if (a.token) {
    if (!a.tokenExp) tokenStatus = "invalid";
    else if (a.tokenExp < Date.now()) tokenStatus = "expired";
    else tokenStatus = "valid";
  }
  return {
    id: a.id,
    name: a.name,
    username: a.username,
    tokenStatus,
    tokenExp: a.tokenExp ? new Date(a.tokenExp).toISOString() : null,
  };
}

export const accountApi = {
  /* ─── دریافت لیست ─── */
  GetList: async (): Promise<AccountInfoView[]> => {
    if (USE_MOCK) {
      await mockDelay(200);
      return mockLoad<MockAccount[]>(MOCK_KEY, []).map(toView);
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<AccountInfoView[]>(
      "/api/Trader/Account/GetList",
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── لیست انتخابی ─── */
  GetSelectionList: async (): Promise<SelectionListDto[]> => {
    if (USE_MOCK) {
      await mockDelay(150);
      return mockLoad<MockAccount[]>(MOCK_KEY, []).map((a) => ({
        value: a.id,
        label: a.name,
        display: a.name,
      }));
    }
    const api = getAPI(API_MODULE);
    const response = await api.get<SelectionListDto[]>(
      "/api/Trader/Account/GetSelectionList",
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── ایجاد ─── */
  create: async (data: CreateAccountCommand): Promise<string> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<MockAccount[]>(MOCK_KEY, []);
      const acc: MockAccount = {
        id: mockUid(),
        name: data.name,
        username: data.username,
        password: data.password,
        token: "",
        tokenExp: null,
      };
      items.push(acc);
      mockSave(MOCK_KEY, items);
      return acc.id;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<string>(
      "/api/Trader/Account/create",
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── ویرایش ─── */
  update: async (data: UpdateAccountCommand): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      const items = mockLoad<MockAccount[]>(MOCK_KEY, []);
      const idx = items.findIndex((a) => a.id === data.id);
      if (idx < 0) throw new Error("حساب پیدا نشد");
      items[idx] = {
        ...items[idx],
        name: data.name,
        username: data.username,
        ...(data.password ? { password: data.password } : {}),
      };
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.put<boolean>(
      `/api/Trader/Account/${data.id}`,
      data,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── حذف ─── */
  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(200);
      const items = mockLoad<MockAccount[]>(MOCK_KEY, []);
      mockSave(
        MOCK_KEY,
        items.filter((a) => a.id !== id),
      );
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.delete<boolean>(
      `/api/Trader/Account/${id}`,
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── لاگین ─── */
  login: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(800);
      const items = mockLoad<MockAccount[]>(MOCK_KEY, []);
      const idx = items.findIndex((a) => a.id === id);
      if (idx < 0) throw new Error("حساب پیدا نشد");
      if (!items[idx].username || !items[idx].password)
        throw new Error("نام‌کاربری یا رمز خالیه");
      items[idx].token = `mock-jwt-${mockUid()}`;
      items[idx].tokenExp = Date.now() + 12 * 3600 * 1000;
      mockSave(MOCK_KEY, items);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/Account/login",
      { id },
      { withCredentials: true },
    );
    return response.data;
  },

  /* ─── فعال‌سازی (same-login) ─── */
  activate: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      await mockDelay(300);
      return true;
    }
    const api = getAPI(API_MODULE);
    const response = await api.post<boolean>(
      "/api/Trader/Account/activate",
      { id },
      { withCredentials: true },
    );
    return response.data;
  },
};