import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type AxiosResponse,
} from 'axios';
import { auth0Service } from '@/services/auth/auth0Service';
import { type Result, ok, err } from '@/types';

// Define a custom error type for API errors
export interface ApiError {
  message: string;
  statusCode?: number;
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

const axiosInstance = axios.create({
  //baseURL: import.meta.env.VITE_API_BASE_URL,
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
    if (response.data && typeof response.data === 'object' && 'isSuccess' in response.data) {
      const backendResult = response.data as Result<T, ApiError>;
      if (backendResult.isSuccess) {
        return ok(backendResult.value as T);
      } else {
        return err(backendResult.error as ApiError);
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
