import type { Family, FamilySearchFilter } from '@/types/family';
import type { Paginated } from '@/types/pagination';
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types/result';
import type { ApiError } from '@/utils/api';

export interface IFamilyService extends ICrudService<Family> {
  searchItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>>; // Keep searchFamilies
}
