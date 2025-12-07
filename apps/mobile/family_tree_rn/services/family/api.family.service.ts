// apps/mobile/family_tree_rn/services/family/api.family.service.ts

import { ApiClientMethods } from '@/types';
import { Result, ApiError, FamilyDetailDto, PaginatedList, FamilyListDto, SearchPublicFamiliesQuery } from '@/types';
import { IFamilyService } from '@/services/family/family.service.interface';

export class ApiFamilyService implements IFamilyService {
  constructor(private api: ApiClientMethods) {}

  async getFamilyById(id: string): Promise<Result<FamilyDetailDto>> {
    try {
      const response = await this.api.get<FamilyDetailDto>(`/family/${id}`);
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }

  async searchFamilies(query: SearchPublicFamiliesQuery): Promise<Result<PaginatedList<FamilyListDto>>> {
    try {
      const response = await this.api.get<PaginatedList<FamilyListDto>>('/families/search', {
        params: query,
      });
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }
}
