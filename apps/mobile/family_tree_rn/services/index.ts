// apps/mobile/family_tree_rn/services/index.ts

import axios, { AxiosInstance } from 'axios';
import { ApiClientMethods } from '@/types/apiClient';
import { ApiUserProfileService } from './userProfile/api.userProfile.service';
import { authService } from './authService';

const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL;

// Create an Axios instance
const axiosInstance: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to include the access token
axiosInstance.interceptors.request.use(
  async (config) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Implement ApiClientMethods using the Axios instance
const apiClient: ApiClientMethods = {
  get: async <T>(url: string, config?: any) => {
    const response = await axiosInstance.get<T>(url, config);
    return response.data;
  },
  post: async <T>(url: string, data?: any, config?: any) => {
    const response = await axiosInstance.post<T>(url, data, config);
    return response.data;
  },
  put: async <T>(url: string, data?: any, config?: any) => {
    const response = await axiosInstance.put<T>(url, data, config);
    return response.data;
  },
  delete: async <T>(url: string, config?: any) => {
    const response = await axiosInstance.delete<T>(url, config);
    return response.data;
  },
};

// Initialize services
export const userProfileService = new ApiUserProfileService(apiClient);
