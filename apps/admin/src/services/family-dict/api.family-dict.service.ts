import {
  type FamilyDict,
  type Result,
  type FamilyDictFilter,
} from '@/types';
import { type IFamilyDictService, type BackendPaginatedResult } from './family-dict.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

export class ApiFamilyDictService implements IFamilyDictService {
  constructor(private http: ApiClientMethods) {}

  async fetch(): Promise<Result<FamilyDict[], ApiError>> {
    const result = await this.http.get<FamilyDict[]>(`/family-dict`);
    return result;
  }

  async getById(id: string): Promise<Result<FamilyDict | undefined, ApiError>> {
    const result = await this.http.get<FamilyDict>(`/family-dict/${id}`);
    return result;
  }

  async getByIds(ids: string[]): Promise<Result<FamilyDict[], ApiError>> {
    const params = new URLSearchParams();
    ids.forEach(id => params.append('ids', id));
    const result = await this.http.get<FamilyDict[]>(`/family-dict/by-ids?${params.toString()}`);
    return result;
  }

  async add(newItem: Omit<FamilyDict, 'id'>): Promise<Result<FamilyDict, ApiError>> {
    const result = await this.http.post<FamilyDict>(`/family-dict`, newItem);
    return result;
  }

  async update(updatedItem: FamilyDict): Promise<Result<FamilyDict, ApiError>> {
    const result = await this.http.put<FamilyDict>(
      `/family-dict/${updatedItem.id}`,
      updatedItem,
    );
    return result;
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/family-dict/${id}`);
  }

  async loadItems(
    filters: FamilyDictFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<BackendPaginatedResult<FamilyDict>, ApiError>> {
    const params = new URLSearchParams();
    if (filters.searchQuery) params.append('q', filters.searchQuery);
    if (filters.lineage) params.append('lineage', filters.lineage.toString());
    if (filters.region) params.append('region', filters.region);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

    params.append('pageNumber', page.toString());
    params.append('pageSize', itemsPerPage.toString());

    const result = await this.http.get<BackendPaginatedResult<FamilyDict>>(
      `/family-dict/search?${params.toString()}`,
    );

    return result;
  }
}