import type { Relationship, Paginated, Result, RelationshipFilter } from '@/types'; // Added Relationship
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IRelationshipService extends ICrudService<Relationship> {
  add(newItem: Relationship): Promise<Result<Relationship, ApiError>>; // Override add method
  loadItems(
    filters: RelationshipFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Relationship>, ApiError>>; // Changed to Paginated<Relationship>
  getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>>; // Changed to Relationship[]
}
