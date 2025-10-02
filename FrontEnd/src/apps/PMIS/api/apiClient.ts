import axios from 'axios';
import { toast } from 'react-hot-toast';
import { createBrowserHistory } from 'history';
import { normalizeDates } from '../utils/date';

const history = createBrowserHistory();
const apiBase = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'; // مقدار پیش‌فرض اضافه شد

const apiClient = axios.create({
  baseURL: apiBase,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
});

apiClient.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers = config.headers || {};
    config.headers['Authorization'] = `Bearer ${token}`;
  }

  if (['post', 'put', 'patch'].includes(config.method!) && config.data) {
    config.data = normalizeDates(config.data);
  }

  if (config.params) {
    config.params = normalizeDates(config.params);
  }
  
  console.info('API Request:', config.method?.toUpperCase(), config.url);
  return config;
});

apiClient.interceptors.response.use(
  res => {
    console.info('API Response:', res.status, res.config.url);
    return res;
  },
  err => {
    const status = err.response?.status;
    console.error('API Error:', status, err.config?.url, err.message);
    
    if (status === 401) {
      toast.error('نشست شما منقضی شده.');
      localStorage.removeItem('authToken');
      history.replace('/login');
    } else if (status >= 500) {
      toast.error('خطای سرور، بعداً دوباره تلاش کنید.');
    } else {
      toast.error(err.response?.data?.message || 'خطای ارتباط با سرور.');
    }
    return Promise.reject(err);
  }
);

export default apiClient;