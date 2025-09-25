import type { Family } from '@/types/family';
import type { ICrudService } from '../common/crud.service.interface';
import type { IFamilyService } from './family.service.interface';
import type { ApiError } from '@/utils/api';
import { safeApiCall } from '@/utils/api';
import type { AxiosInstance } from 'axios';
import type { FamilySearchFilter } from '@/types/family';
import type { Paginated, Result } from '@/types/common';
import { ok, err } from '@/types/common';
import axios from 'axios';

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyService implements IFamilyService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/families`;

  async fetch(): Promise<Result<Family[], ApiError>> { // Renamed from fetchFamilies
    return safeApiCall(axios.get<Family[]>(this.apiUrl));
  }

  async getById(id: string): Promise<Result<Family, ApiError>> { // Renamed from getFamilyById
    return safeApiCall(axios.get<Family>(`${this.apiUrl}/${id}`));
  }

  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> { // Renamed from addFamily
    return safeApiCall(axios.post<Family>(this.apiUrl, newItem));
  }

  async update(updatedItem: Family): Promise<Result<Family, ApiError>> { // Renamed from updateFamily
    return safeApiCall(axios.put<Family>(`${this.apiUrl}/${updatedItem.id}`, updatedItem));
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      await this.http.delete<void>(`/api/family/${id}`);
      return ok(undefined);
    } catch (error: any) {
      return err(error);
    }
  }

  async searchItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>> {
    try {
      const params = new URLSearchParams();
      if (filter.searchQuery) params.append('searchQuery', filter.searchQuery);
      if (filter.familyId) params.append('familyId', filter.familyId);
      if (filter.startDate) params.append('startDate', filter.startDate.toISOString());
      if (filter.endDate) params.append('endDate', filter.endDate.toISOString());
      if (filter.location) params.append('location', filter.location);
      if (filter.type) params.append('type', filter.type);

      params.append('page', page.toString());
      params.append('itemsPerPage', itemsPerPage.toString());

      const response = await this.http.get<Paginated<Family>>(`/api/family/search?${params.toString()}`);
      return ok(response.data);
    } catch (error: any) {
      return err(error);
    }
  }

  async getManyByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    console.log(`Fetching families by IDs: ${ids.join(', ')} from API`);
    const params = new URLSearchParams();
    ids.forEach(id => params.append('ids', id));
    return safeApiCall(this.http.get<Family[]>(`${this.apiUrl}/by-ids?${params.toString()}`));
  }
}