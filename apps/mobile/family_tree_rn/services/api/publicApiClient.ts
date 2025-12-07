// apps/mobile/family_tree_rn/services/api/publicApi.ts

import axios from 'axios';
import { ApiClientMethods } from '@/types';
import { authService } from '@/services/authService';

// TODO: Configure this based on your environment (e.g., .env file)
const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL+'/api/public'; // Example: Replace with your backend URL

export const publicApiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to include the API Key
publicApiClient.interceptors.request.use(
  (config) => {
    const apiKey = process.env.EXPO_PUBLIC_API_KEY;
    if (apiKey) {
      config.headers['X-App-Key'] = apiKey; // Add the API Key header
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Tạo một apiClient có interceptor để thêm Access Token cho các request cần xác thực
export const apiClientWithAuth: ApiClientMethods = {
  get: async <T>(url: string, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.get<T>(url, config);
    delete publicApiClient.defaults.headers.common['Authorization']; // Xóa header sau khi request hoàn tất
    return response.data;
  },
  post: async <T>(url: string, data?: any, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.post<T>(url, data, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
  put: async <T>(url: string, data?: any, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.put<T>(url, data, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
  delete: async <T>(url: string, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.delete<T>(url, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
};

