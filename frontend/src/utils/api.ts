import { type Result, ok, err } from '@/types';
import axios, { type AxiosError, type AxiosRequestConfig, type AxiosResponse } from 'axios';

// Define a custom error type for API errors
export interface ApiError {
  message: string;
  statusCode?: number;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  details?: any;
}

// Helper function to create an ApiError from an AxiosError
const createApiError = (error: AxiosError): ApiError => {
  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    return {
      message: (error.response?.data as any)?.message || error.message,
      statusCode: error.response?.status,
      details: error.response?.data,
    };
  } else if (error.request) {
    // The request was made but no response was received
    return {
      message: 'No response received from server.',
      details: error.request,
    };
  } else {
    // Something happened in setting up the request that triggered an Error
    return {
      message: error.message,
    };
  }
};

export async function safeApiCall<T>(apiCall: Promise<AxiosResponse<T>>): Promise<Result<T, ApiError>> {
  try {
    const response = await apiCall;
    return ok(response.data);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      return err(createApiError(error));
    } else {
      return err({ message: 'An unexpected error occurred.', details: error as Error });
    }
  }
}

// You can also create specific wrappers for common HTTP methods if preferred
export const api = {
  get: async <T>(url: string, config?: AxiosRequestConfig): Promise<Result<T, ApiError>> => {
    return safeApiCall(axios.get<T>(url, config));
  },
  post: async <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<Result<T, ApiError>> => {
    return safeApiCall(axios.post<T>(url, data, config));
  },
  put: async <T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<Result<T, ApiError>> => {
    return safeApiCall(axios.put<T>(url, data, config));
  },
  delete: async <T>(url: string, config?: AxiosRequestConfig): Promise<Result<T, ApiError>> => {
    return safeApiCall(axios.delete<T>(url, config));
  },
};
