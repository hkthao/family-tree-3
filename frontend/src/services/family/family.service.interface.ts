import type { Family } from '@/types/family';
import type { Paginated } from '@/types/pagination'; // Import generic Paginated interface

export interface IFamilyService {
  fetchFamilies(): Promise<Family[]>;
  getFamilyById(id: string): Promise<Family | undefined>;
  addFamily(newFamily: Omit<Family, 'id'>): Promise<Family>;
  updateFamily(updatedFamily: Family): Promise<Family>;
  deleteFamily(id: string): Promise<void>;
  searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>>; // Use generic Paginated interface
}
