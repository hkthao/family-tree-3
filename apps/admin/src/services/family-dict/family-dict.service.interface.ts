import type { FamilyDict, FamilyDictFilter, FamilyDictImport, Paginated } from '@/types'; // Changed Paginated to Paginated
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types'; // Added type keyword

export interface IFamilyDictService extends ICrudService<FamilyDict> {
  loadItems(
    filters: FamilyDictFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<FamilyDict>>>; // Changed Paginated to Paginated, removed ApiError

  importItems(data: FamilyDictImport): Promise<Result<string[]>>; // Removed ApiError
}