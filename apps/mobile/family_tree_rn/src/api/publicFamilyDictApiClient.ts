import axios from 'axios';
import type {
  FamilyDictDto,
  PaginatedFamilyDictDto,
  FamilyDictFilter,
} from '@/types/public.d';

const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL+'/api/public';
const publicApiClient = axios.create({
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

export const getPublicFamilyDicts = async (
  filter: FamilyDictFilter,
  page: number,
  itemsPerPage: number,
): Promise<PaginatedFamilyDictDto> => {
  const response = await publicApiClient.get<PaginatedFamilyDictDto>('/family-dict', {
    params: {
      page: page,
      itemsPerPage: itemsPerPage,
      searchQuery: filter.searchQuery,
      lineage: filter.lineage,
      region: filter.region,
      sortBy: filter.sortBy,
      sortOrder: filter.sortOrder,
    },
  });
  return response.data;
};

export const getPublicFamilyDictById = async (id: string): Promise<FamilyDictDto> => {
  const response = await publicApiClient.get<FamilyDictDto>(`/family-dict/${id}`);
  return response.data;
};