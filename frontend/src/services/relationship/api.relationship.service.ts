import type { IRelationshipService } from './relationship.service.interface';
import {
  type Relationship,
  type Paginated,
  type Result,
  type RelationshipFilter,
  ok,
} from '@/types'; // Added Relationship
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export class ApiRelationshipService implements IRelationshipService {
  private mapListDtoToRelationship(dto: Relationship): Relationship {
    return {
      id: dto.id,
      sourceMemberId: dto.sourceMemberId,
      targetMemberId: dto.targetMemberId,
      type: dto.type,
      order: dto.order,
      sourceMember: dto.sourceMember,
      targetMember: dto.targetMember,
    };
  }

  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/relationships`;

  async fetch(): Promise<Result<Relationship[], ApiError>> {
    const result = await this.http.get<Relationship[]>(this.apiUrl);
    if (result.ok) {
      return ok(result.value.map(this.mapListDtoToRelationship));
    }
    return result;
  }

  async getById(
    id: string,
  ): Promise<Result<Relationship | undefined, ApiError>> {
    return this.http.get<Relationship>(`${this.apiUrl}/${id}`);
  }

  async add(
    newItem: Relationship,
  ): Promise<Result<Relationship, ApiError>> {
    return this.http.post<Relationship>(this.apiUrl, newItem);
  }

  async update(
    updatedItem: Relationship,
  ): Promise<Result<Relationship, ApiError>> {
    return this.http.put<Relationship>(
      `${this.apiUrl}/${updatedItem.id}`,
      updatedItem,
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  async loadItems(
    filters: RelationshipFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Relationship>, ApiError>> {
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.sourceMemberId)
      params.append('sourceMemberId', filters.sourceMemberId);
    if (filters.targetMemberId)
      params.append('targetMemberId', filters.targetMemberId);
    if (filters.type) params.append('type', filters.type);
    if (filters.familyId) params.append('familyId', filters.familyId);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return this.http.get<Paginated<Relationship>>(
      `${this.apiUrl}/search?${params.toString()}`,
    );
  }

  async getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<Relationship[]>(
      `${this.apiUrl}/by-ids?${params.toString()}`,
    );
  }
}
