import apiClient from '@/plugins/axios';
import type { Result } from '@/types';
import type { ApiError } from '@/types/apiError';
import { useAuthStore } from '@/stores/auth.store';

export const useEventService = () => {
  const authStore = useAuthStore();

  const generateEventOccurrences = async (year: number, familyId: string): Promise<Result<string>> => { // MODIFIED: Added familyId parameter
    try {
      if (!authStore.isAdmin) {
        return { ok: false, error: { name: 'ApiError', message: 'Unauthorized: Only administrators can perform this action.' } };
      }
      // MODIFIED: Corrected URL path and added familyId query parameter
      const response = await apiClient.post<string>(`/event/generate-occurrences?year=${year}&familyId=${familyId}`);
      return response;
    } catch (error: any) {
      return { ok: false, error: { name: 'ApiError', message: error.message || 'Failed to generate event occurrences.' } };
    }
  };

  return {
    generateEventOccurrences,
  };
};