import type { Relationship, Paginated, Result, RelationshipFilter, RelationshipDetectionResult } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IRelationshipService extends ICrudService<Relationship> {
  add(newItem: Omit<Relationship, 'id'>): Promise<Result<Relationship, ApiError>>; // Override add method
  loadItems(
    filters: RelationshipFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Relationship>, ApiError>>; // Changed to Paginated<Relationship>
  getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>>; // Changed to Relationship[]
  addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>>; // New method for bulk adding relationships
  detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<RelationshipDetectionResult | null>; // NEW
}
