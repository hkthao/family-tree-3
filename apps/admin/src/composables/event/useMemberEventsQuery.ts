import { useQuery } from '@tanstack/vue-query';
import type { EventDto, ApiError, Result } from '@/types';
import { DefaultEventServiceAdapter } from './event.adapter'; // Import the adapter

export function useMemberEventsQuery(memberId: string) {
  return useQuery({
    queryKey: ['member-events', memberId],
    queryFn: async (): Promise<EventDto[]> => {
      const response: Result<EventDto[], ApiError> = await DefaultEventServiceAdapter.getEventsByMemberId(memberId);
      if (response.ok) {
        return response.value;
      }
      throw new Error(response.error?.message || 'Failed to fetch member events');
    },
    enabled: !!memberId,
  });
}