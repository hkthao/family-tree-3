import type { Family, FamilySearchFilter } from '@/types/family';
import type { Paginated } from '@/types/pagination';
import type { ICrudService } from '../common/crud.service.interface';

export interface IFamilyService extends ICrudService<Family> {
  searchItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>>; // Keep searchFamilies
}
