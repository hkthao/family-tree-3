import type { IFamilyLinkRequestService } from './familyLinkRequest.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/plugins/axios';
import type { FamilyLinkRequestDto, FamilyLinkRequestFilter, Paginated, Result } from '@/types';

export class ApiFamilyLinkRequestService implements IFamilyLinkRequestService {
  constructor(private apiClient: ApiClientMethods) { }

  async searchFamilyLinkRequests(familyId: string, filters: FamilyLinkRequestFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkRequestDto>, ApiError>> {
    const params = new URLSearchParams();
    params.append('familyId', familyId);
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());
    if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
    if (filters.status) params.append('status', filters.status?.toString());
    if (filters.otherFamilyId) params.append('otherFamilyId', filters.otherFamilyId);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

    return await this.apiClient.get<Paginated<FamilyLinkRequestDto>>(`/family-link-requests/search`, { params });
  }

  async getFamilyLinkRequestById(id: string): Promise<Result<FamilyLinkRequestDto, ApiError>> {
    return await this.apiClient.get<FamilyLinkRequestDto>(`/family-link-requests/${id}`);
  }

  async createFamilyLinkRequest(requestingFamilyId: string, targetFamilyId: string, requestMessage?: string): Promise<Result<string, ApiError>> {
    return await this.apiClient.post<string>('/family-link-requests', { requestingFamilyId, targetFamilyId, requestMessage });
  }

  async approveFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>> {
    return await this.apiClient.post<void>(`/family-link-requests/${requestId}/approve`, { responseMessage });
  }

  async rejectFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>> {
    return await this.apiClient.post<void>(`/family-link-requests/${requestId}/reject`, { responseMessage });
  }

  async deleteFamilyLinkRequest(id: string): Promise<Result<void, ApiError>> {
    return await this.apiClient.delete<void>(`/family-link-requests/${id}`);
  }
}
