import type { IFamilyService } from './family.service.interface';
import type { ApiError } from '@/utils/api';
import { safeApiCall } from '@/utils/api';
import type { AxiosInstance } from 'axios';
import {
  ok,
  err,
  type Result,
  type Family,
  type FamilyFilter,
  type Paginated,
} from '@/types';

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyService implements IFamilyService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/family`;

  async fetch(): Promise<Result<Family[], ApiError>> {
    // Renamed from fetchFamilies
    return safeApiCall(this.http.get<Family[]>(this.apiUrl));
  }

  async getById(id: string): Promise<Result<Family, ApiError>> {
    // Renamed from getById
    return safeApiCall(this.http.get<Family>(`${this.apiUrl}/${id}`));
  }

  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    // Renamed from addFamily
    return safeApiCall(this.http.post<Family>(this.apiUrl, newItem));
  }

  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    // Renamed from updateFamily
    return safeApiCall(
      this.http.put<Family>(`${this.apiUrl}/${updatedItem.id}`, updatedItem),
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      await this.http.delete<void>(`${this.apiUrl}/${id}`);
      return ok(undefined);
    } catch (error: any) {
      return err(error);
    }
  }

  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    try {
      const params = new URLSearchParams();
      if (filter.searchQuery) params.append('searchQuery', filter.searchQuery);
      if (filter.familyId) params.append('familyId', filter.familyId);
      params.append('page', page.toString());
      params.append('itemsPerPage', itemsPerPage.toString());

      const response = await this.http.get<Paginated<Family>>(
        `/api/family/search?${params.toString()}`,
      );
      return ok(response.data);
    } catch (error: any) {
      return err(error);
    }
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    console.log(`Fetching families by IDs: ${ids.join(', ')} from API`);
    const params = new URLSearchParams();
    ids.forEach((id) => params.append('ids', id));
    return safeApiCall(
      this.http.get<Family[]>(`${this.apiUrl}/by-ids?${params.toString()}`),
    );
  }
}
