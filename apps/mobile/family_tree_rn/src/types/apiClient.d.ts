// apps/mobile/family_tree_rn/src/types/apiClient.d.ts

import { AxiosRequestConfig } from 'axios';

// Simplified interface for API client methods
export interface ApiClientMethods {
  get<T>(url: string, config?: AxiosRequestConfig): Promise<T>;
  post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T>;
  put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T>;
  delete<T>(url: string, config?: AxiosRequestConfig): Promise<T>;
}
