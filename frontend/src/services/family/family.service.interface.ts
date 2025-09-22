import type { Family } from '@/types/family';
import type { Paginated } from '@/types/pagination'; // Import generic Paginated interface
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService

export interface IFamilyService extends ICrudService<Family> { // Extend ICrudService
  searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>>; // Keep searchFamilies
}
