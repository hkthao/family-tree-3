import type { IRelationshipService } from './relationship.service.interface';
import type { Relationship, RelationshipFilter } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { ok, type Paginated, type Result } from '@/types';

export class ApiRelationshipService implements IRelationshipService {

  constructor(private http: ApiClientMethods) {}

  async getById(
    id: string,
  ): Promise<Result<Relationship | undefined, ApiError>> {
    return this.http.get<Relationship>(`/relationship/${id}`);
  }

  async add(
    newItem: Relationship,
  ): Promise<Result<Relationship, ApiError>> {
    return this.http.post<Relationship>(`/relationship`, newItem);
  }

  async update(
    updatedItem: Relationship,
  ): Promise<Result<Relationship, ApiError>> {
    return this.http.put<Relationship>(
      `/relationship/${updatedItem.id}`,
      updatedItem,
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/relationship/${id}`);
  }

  async loadItems(
    filters: RelationshipFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Relationship>, ApiError>> {
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
    if (filters.sourceMemberId)
      params.append('sourceMemberId', filters.sourceMemberId);
    if (filters.targetMemberId)
      params.append('targetMemberId', filters.targetMemberId);
    if (filters.type) params.append('type', filters.type);
    if (filters.familyId) params.append('familyId', filters.familyId);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return this.http.get<Paginated<Relationship>>(
      `/relationship/search?${params.toString()}`,
    );
  }

  async getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<Relationship[]>(
      `/relationship/by-ids?${params.toString()}`,
    );
  }

  async addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>> {
    const payload = { relationships: newItems };
    return this.http.post<string[]>(`/relationship/bulk-create`, payload);
  }
}
