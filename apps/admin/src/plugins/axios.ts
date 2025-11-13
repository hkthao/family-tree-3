import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type AxiosResponse,
} from 'axios';
import { auth0Service } from '@/services/auth/auth0Service';
import { type Result, ok, err } from '@/types';

// Define a custom error type for API errors
export interface ApiError {
  name: string;
  message: string;
  statusCode?: number;
  details?: any;
}

// Helper function to create an ApiError from an AxiosError
const createApiError = (error: AxiosError): ApiError => {
  if (error.response) {
    let errorMessage = error.message;
    if (error.response.data) {
      if (typeof error.response.data === 'string') {
        errorMessage = error.response.data;
      } else if ((error.response.data as any).message) {
        errorMessage = (error.response.data as any).message;
      } else if ((error.response.data as any).details) {
        errorMessage = (error.response.data as any).details;
      }
    }
    return {
      name: 'ApiError',
      message: errorMessage,
      statusCode: error.response.status,
      details: error.response.data,
    };
  } else if (error.request) {
    return {
      name: 'ApiError',
      message: 'No response received from server.',
      details: error.request,
    };
  } else {
    return {
      name: 'ApiError',
      message: error.message,
    };
  }
};

const axiosInstance = axios.create({
  baseURL: window.runtimeConfig.VITE_API_BASE_URL,
  headers: {
    'Content-type': 'application/json',
  },
});

axiosInstance.interceptors.request.use(async (config) => {
  const token = await auth0Service.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

async function safeApiCall<T>(
  apiCall: Promise<AxiosResponse<any>>,
): Promise<Result<T, ApiError>> {
  try {
    const response = await apiCall;
    // Check if the response data is already a Result object from the backend
    if (
      response.data &&
      typeof response.data === 'object' &&
      'isSuccess' in response.data
    ) {
      if (response.data.isSuccess) {
        return ok(response.data.value as T);
      } else {
        return err(response.data.error as ApiError);
      }
    } else {
      // Otherwise, assume the data is the direct payload
      return ok(response.data as T);
    }
  } catch (error) {
    if (axios.isAxiosError(error)) {
      return err(createApiError(error));
    } else {
      return err({
        name: 'ApiError',
        message: 'An unexpected error occurred.',
        details: error as Error,
      });
    }
  }
}

export interface ApiClientMethods {
  get<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>>;
  post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>>;
  put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>>;
  delete<T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>>;
}

export const apiClient: ApiClientMethods = {
  get: async <T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>> => {
    return safeApiCall(axiosInstance.get<T>(url, config));
  },
  post: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>> => {
    return safeApiCall(axiosInstance.post<T>(url, data, config));
  },
  put: async <T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>> => {
    return safeApiCall(axiosInstance.put<T>(url, data, config));
  },
  delete: async <T>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<Result<T, ApiError>> => {
    return safeApiCall(axiosInstance.delete<T>(url, config));
  },
};

export default apiClient;
