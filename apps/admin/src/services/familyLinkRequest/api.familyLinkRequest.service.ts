import type { IFamilyLinkRequestService } from './familyLinkRequest.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/plugins/axios';
import type { FamilyLinkRequestDto, FamilyLinkRequestFilter, Paginated, Result } from '@/types';

export class ApiFamilyLinkRequestService implements IFamilyLinkRequestService {
  constructor(private apiClient: ApiClientMethods) { }

  async searchFamilyLinkRequests(familyId: string, filters: FamilyLinkRequestFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkRequestDto>, ApiError>> {
    try {
      const params = new URLSearchParams();
      params.append('familyId', familyId);
      params.append('pageNumber', page.toString());
      params.append('pageSize', itemsPerPage.toString());
      if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
      if (filters.status) params.append('status', filters.status);
      if (filters.otherFamilyId) params.append('otherFamilyId', filters.otherFamilyId);
      if (filters.sortBy) params.append('sortBy', filters.sortBy);
      if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

      const response = await this.apiClient.get<Paginated<FamilyLinkRequestDto>>(`/family-link-requests/family/${familyId}`, { params });
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async getFamilyLinkRequestById(id: string): Promise<Result<FamilyLinkRequestDto, ApiError>> {
    try {
      const response = await this.apiClient.get<FamilyLinkRequestDto>(`/family-link-requests/${id}`);
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async createFamilyLinkRequest(requestingFamilyId: string, targetFamilyId: string, requestMessage?: string): Promise<Result<string, ApiError>> {
    const response = await this.apiClient.post<string>('/family-link-requests', { requestingFamilyId, targetFamilyId, requestMessage });
    return response;
  }

  async approveFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>> {
    try {
      const response = await this.apiClient.post<void>(`/family-link-requests/${requestId}/approve`, { responseMessage });
      return response;
    } catch (error: any) {
        return { ok: false, error: error };
    }
  }

  async rejectFamilyLinkRequest(requestId: string, responseMessage?: string): Promise<Result<void, ApiError>> {
    try {
      const response = await this.apiClient.post<void>(`/family-link-requests/${requestId}/reject`, { responseMessage });
      return response;
    } catch (error: any) {
        return { ok: false, error: error };
    }
  }

  async deleteFamilyLinkRequest(id: string): Promise<Result<void, ApiError>> {
    try {
      const response = await this.apiClient.delete<void>(`/family-link-requests/${id}`);
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }
}
