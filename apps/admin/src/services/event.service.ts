import apiClient from '@/plugins/axios';
import type { Result } from '@/types'; // Result is a generic type
import type { ApiError } from '@/types/apiError'; // Specific import for ApiError
import { useAuthStore } from '@/stores/auth.store';

export const useEventService = () => {
  const authStore = useAuthStore();

  const generateEventOccurrences = async (year: number): Promise<Result<string>> => {
    try {
      if (!authStore.isAdmin) {
        return { ok: false, error: { name: 'ApiError', message: 'Unauthorized: Only administrators can perform this action.' } }; // Direct ApiError object
      }
      const response = await apiClient.post<string>(`/api/event/generate-occurrences?year=${year}`);
      return response; // Simply return the Result object from apiClient
    } catch (error: any) {
      return { ok: false, error: { name: 'ApiError', message: error.message || 'Failed to generate event occurrences.' } }; // Direct ApiError object
    }
  };

  return {
    generateEventOccurrences,
  };
};
