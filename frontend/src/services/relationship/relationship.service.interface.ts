import type { Relationship, Paginated, Result, RelationshipFilter } from '@/types';
import type { ApiError } from '@/utils/api';

export interface IRelationshipService {
  fetch(): Promise<Result<Relationship[], ApiError>>;
  getById(id: string): Promise<Result<Relationship | undefined, ApiError>>;
  add(newItem: Omit<Relationship, 'id'>): Promise<Result<Relationship, ApiError>>;
  update(updatedItem: Relationship): Promise<Result<Relationship, ApiError>>;
  delete(id: string): Promise<Result<void, ApiError>>;
  loadItems(
    filters: RelationshipFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Relationship>, ApiError>>;
  getByIds(ids: string[]): Promise<Result<Relationship[], ApiError>>;
}
