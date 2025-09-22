import type { Family } from '@/types/family';

export interface PaginatedFamilies {
  items: Family[];
  totalItems: number;
  totalPages: number;
}

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
  ): Promise<PaginatedFamilies>;
}
