import { apiClient } from '@/plugins/axios';
import type { LocationLinkDto } from '@/types/location-link';
import type { Result } from '@/types/result'; // Import Result type
import type { ApiError } from '@/types/apiError'; // Import ApiError type

/**
 * Fetches location links associated with a specific member.
 * @param memberId The ID of the member.
 * @returns A promise that resolves to an array of LocationLinkDto.
 */
export async function fetchLocationLinksByMemberId(memberId: string): Promise<LocationLinkDto[]> {
  const response: Result<LocationLinkDto[], ApiError> = await apiClient.get<LocationLinkDto[]>(`/location-links/by-member/${memberId}`);
  if (response.ok) { // Check response.ok
    return response.value;
  } else {
    throw response.error;
  }
}
