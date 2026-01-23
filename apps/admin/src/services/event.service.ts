import apiClient from '@/plugins/axios';
import type { Result, GenerateAndNotifyEventsCommand } from '@/types';
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

  const sendEventNotification = async (eventId: string): Promise<Result<string>> => {
    try {
      if (!authStore.isAdmin) {
        return { ok: false, error: { name: 'ApiError', message: 'Unauthorized: Only administrators can perform this action.' } };
      }
      const response = await apiClient.post<string>(`/event/${eventId}/send-notification`);
      return response;
    } catch (error: any) {
      return { ok: false, error: { name: 'ApiError', message: error.message || 'Failed to send event notification.' } };
    }
  };

  const generateAndNotifyEvents = async (command: GenerateAndNotifyEventsCommand): Promise<Result<string>> => {
    try {
      if (!authStore.isAdmin) {
        return { ok: false, error: { name: 'ApiError', message: 'Unauthorized: Only administrators can perform this action.' } };
      }
      const response = await apiClient.post<string>(`/event/generate-and-notify`, command); // Assuming /event route prefix
      return response;
    } catch (error: any) {
      return { ok: false, error: { name: 'ApiError', message: error.message || 'Failed to generate and notify events.' } };
    }
  };

  return {
    generateEventOccurrences,
    sendEventNotification,
    generateAndNotifyEvents,
  };
};