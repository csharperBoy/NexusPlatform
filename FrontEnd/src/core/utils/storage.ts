// core/utils/storage.ts
const TOKEN_KEY = "token";
const USER_KEY = "user";


export const storage = {
  getToken: () => localStorage.getItem(TOKEN_KEY),
  setToken: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clearToken: () => localStorage.removeItem(TOKEN_KEY),

  setUser: (user: { email: string }) =>    localStorage.setItem(USER_KEY, JSON.stringify(user)),
  getUser: (): { email: string } | null => {
    const u = localStorage.getItem(USER_KEY);
    return u ? JSON.parse(u) : null;
  },
  clearUser: () => localStorage.removeItem(USER_KEY),
};
