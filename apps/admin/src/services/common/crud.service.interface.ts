import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface ICrudService<T> {
  getById(id: string): Promise<Result<T | undefined, ApiError>>;
  add(newItem: Omit<T, 'id'>): Promise<Result<T, ApiError>>;
  update(updatedItem: T): Promise<Result<T, ApiError>>;
  delete(id: string): Promise<Result<void, ApiError>>;
  getByIds(ids: string[]): Promise<Result<T[], ApiError>>; // New method for fetching multiple items by IDs
}
