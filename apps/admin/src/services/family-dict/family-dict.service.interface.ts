import type { FamilyDict, FamilyDictFilter, FamilyDictImport, PaginatedList } from '@/types'; // Changed Paginated to PaginatedList
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types'; // Added type keyword

export interface IFamilyDictService extends ICrudService<FamilyDict> {
  loadItems(
    filters: FamilyDictFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<PaginatedList<FamilyDict>>>; // Changed Paginated to PaginatedList, removed ApiError

  importItems(data: FamilyDictImport): Promise<Result<string[]>>; // Removed ApiError
}