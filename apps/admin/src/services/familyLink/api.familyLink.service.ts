import type { IFamilyLinkService } from './familyLink.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/plugins/axios';
import type { FamilyLinkDto, FamilyLinkFilter, Paginated, Result } from '@/types';

export class ApiFamilyLinkService implements IFamilyLinkService {
  constructor(private apiClient: ApiClientMethods) { }

  async getFamilyLinks(familyId: string, filters: FamilyLinkFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<FamilyLinkDto>, ApiError>> {
    try {
      const params = new URLSearchParams();
      params.append('familyId', familyId); // Add familyId to query params
      params.append('pageNumber', page.toString());
      params.append('pageSize', itemsPerPage.toString());
      if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
      if (filters.otherFamilyId) params.append('otherFamilyId', filters.otherFamilyId);
      if (filters.sortBy) params.append('sortBy', filters.sortBy);
      if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

      const response = await this.apiClient.get<Paginated<FamilyLinkDto>>(`/family-link/search`, { params }); // Change endpoint
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async getFamilyLinkById(familyLinkId: string): Promise<Result<FamilyLinkDto, ApiError>> {
    try {
      const response = await this.apiClient.get<FamilyLinkDto>(`/family-link/links/by-id/${familyLinkId}`);
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async deleteFamilyLink(familyLinkId: string): Promise<Result<void, ApiError>> {
    try {
      const response = await this.apiClient.delete<void>(`/family-link/delete/${familyLinkId}`);
      return response;
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }
}