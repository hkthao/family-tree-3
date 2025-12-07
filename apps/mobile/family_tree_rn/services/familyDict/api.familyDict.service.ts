// apps/mobile/family_tree_rn/services/familyDict/api.familyDict.service.ts

import { ApiClientMethods } from '@/types';
import { Result, ApiError, FamilyDictDto, PaginatedList, FamilyDictFilter } from '@/types';
import { IFamilyDictService } from '@/services/familyDict/familyDict.service.interface';

export class ApiFamilyDictService implements IFamilyDictService {
  constructor(private api: ApiClientMethods) {}

  async getFamilyDicts(filter: FamilyDictFilter, page: number, itemsPerPage: number): Promise<Result<PaginatedList<FamilyDictDto>>> {
    try {
      const response = await this.api.get<PaginatedList<FamilyDictDto>>('/family-dict', {
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
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }

  async getFamilyDictById(id: string): Promise<Result<FamilyDictDto>> {
    try {
      const response = await this.api.get<FamilyDictDto>(`/family-dict/${id}`);
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
