import type { IRelationshipService } from './relationship.service.interface';
import type { Relationship, Paginated, Result, RelationshipFilter } from '@/types';
import { safeApiCall } from '@/utils/api';
import type { ApiError } from '@/utils/api';
import type { AxiosInstance } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiRelationshipService implements IRelationshipService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/relationships`;

  async fetch(): Promise<Result<Relationship[], ApiError>> {
    return safeApiCall(this.http.get<Relationship[]>(this.apiUrl));
  }

  async getById(id: string): Promise<Result<Relationship | undefined, ApiError>> {
    return safeApiCall(this.http.get<Relationship>(`${this.apiUrl}/${id}`));
  }

  async add(newItem: Omit<Relationship, 'id'>): Promise<Result<Relationship, ApiError>> {
    return safeApiCall(this.http.post<Relationship>(this.apiUrl, newItem));
  }

  async update(updatedItem: Relationship): Promise<Result<Relationship, ApiError>> {
    return safeApiCall(
      this.http.put<Relationship>(`${this.apiUrl}/${updatedItem.id}`, updatedItem),
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return safeApiCall(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }

  async loadItems(
    filters: RelationshipFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Relationship>, ApiError>> {
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.sourceMemberId) params.append('sourceMemberId', filters.sourceMemberId);
    if (filters.targetMemberId) params.append('targetMemberId', filters.targetMemberId);
    if (filters.type) params.append('type', filters.type);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return safeApiCall(
      this.http.get<Paginated<Relationship>>(`${this.apiUrl}/search?${params.toString()}`),
    );
  }

  async getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>> {
    const params = new URLSearchParams();
    ids.forEach((id) => params.append('ids', id));
    return safeApiCall(
      this.http.get<Relationship[]>(`${this.apiUrl}/by-ids?${params.toString()}`),
    );
  }
}
