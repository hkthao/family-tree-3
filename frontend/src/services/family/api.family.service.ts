import type { Family } from '@/types/family';
import type { IFamilyService } from './family.service.interface';
import { safeApiCall } from '@/utils/api';
import type { ApiError } from '@/utils/api';
import type { Result } from '@/types/common';
import axios from 'axios';
import type { Paginated } from '@/types/common'; // Correct placement of import

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyService implements IFamilyService {
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

  async delete(id: string): Promise<Result<void, ApiError>> { // Renamed from deleteFamily
    return safeApiCall(axios.delete<void>(`${this.apiUrl}/${id}`));
  }

  async searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>> {
    const params = new URLSearchParams();
    if (searchQuery) params.append('searchQuery', searchQuery);
    if (visibility !== 'all') params.append('visibility', visibility);
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return safeApiCall(axios.get<Paginated<Family>>(`${this.apiUrl}?${params.toString()}`));
  }
}