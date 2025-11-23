import type { IFamilyService } from './family.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import {
  type Result,
  type Family,
  type FamilyFilter,
  type Paginated,
} from '@/types';

export class ApiFamilyService implements IFamilyService {
  constructor(private http: ApiClientMethods) {}

  async fetch(): Promise<Result<Family[], ApiError>> {
    // Renamed from fetchFamilies
    return this.http.get<Family[]>(`/family`);
  }

  async getById(id: string): Promise<Result<Family, ApiError>> {
    // Renamed from getById
    return this.http.get<Family>(`/family/${id}`);
  }

  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    // Renamed from addFamily
    return this.http.post<Family>(`/family`, newItem);
  }

  async addItems(
    newItems: Omit<Family, 'id'>[],
  ): Promise<Result<string[], ApiError>> {
    return this.http.post<string[]>(`/family/bulk-create`, {
      families: newItems,
    });
  }

  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    // Renamed from updateFamily
    return this.http.put<Family>(
      `/family/${updatedItem.id}`,
      updatedItem,
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/family/${id}`);
  }

  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    const params = new URLSearchParams();
    if (filter.searchQuery) params.append('searchQuery', filter.searchQuery);
    if (filter.familyId) params.append('familyId', filter.familyId);
    if (filter.visibility) params.append('visibility', filter.visibility);
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());
    if (filter.sortBy) params.append('sortBy', filter.sortBy);
    if (filter.sortOrder) params.append('sortOrder', filter.sortOrder);

    return this.http.get<Paginated<Family>>(
      `/family/search?${params.toString()}`,
    );
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<Family[]>(
      `/family/by-ids?${params.toString()}`,
    );
  }

  async getByIdWithDetails(id: string): Promise<Result<Family, ApiError>> {
    return this.http.get<Family>(`/family/${id}/details`); // New endpoint for detailed family data
  }
}
