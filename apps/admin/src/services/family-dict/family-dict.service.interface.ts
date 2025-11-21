import type { FamilyDict, Result, FamilyDictFilter } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

// Define the expected backend paginated result structure
export interface BackendPaginatedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface IFamilyDictService extends ICrudService<FamilyDict> {
  loadItems(
    filters: FamilyDictFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<BackendPaginatedResult<FamilyDict>, ApiError>>;

  importItems(items: Omit<FamilyDict, 'id'>[]): Promise<Result<string[], ApiError>>;
}