import type { Family } from '@/types/family';
import type { IFamilyService } from './family.service.interface';
import axios from 'axios';
import type { Paginated } from '@/types/pagination'; // Correct placement of import

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyService implements IFamilyService {
  private apiUrl = `${API_BASE_URL}/families`;

  async fetch(): Promise<Family[]> { // Renamed from fetchFamilies
    const response = await axios.get<Family[]>(this.apiUrl);
    return response.data;
  }

  async getById(id: string): Promise<Family | undefined> { // Renamed from getFamilyById
    const response = await axios.get<Family>(`${this.apiUrl}/${id}`);
    return response.data;
  }

  async add(newItem: Omit<Family, 'id'>): Promise<Family> { // Renamed from addFamily
    const response = await axios.post<Family>(this.apiUrl, newItem);
    return response.data;
  }

  async update(updatedItem: Family): Promise<Family> { // Renamed from updateFamily
    const response = await axios.put<Family>(`${this.apiUrl}/${updatedItem.id}`, updatedItem);
    return response.data;
  }

  async delete(id: string): Promise<void> { // Renamed from deleteFamily
    await axios.delete(`${this.apiUrl}/${id}`);
  }

  async searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>> {
    const params = new URLSearchParams();
    if (searchQuery) params.append('searchQuery', searchQuery);
    if (visibility !== 'all') params.append('visibility', visibility);
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    const response = await axios.get<Paginated<Family>>(`${this.apiUrl}?${params.toString()}`);
    return response.data;
  }
}