import type { PaginatedList, ListOptions, FilterOptions } from '@/types';
import type { Result } from '@/types'; // Explicitly import Result type

export interface ICrudService<T> {
  search(
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<PaginatedList<T>>>;
  getById(id: string): Promise<Result<T | undefined>>;
  add(newItem: Omit<T, 'id'>): Promise<Result<T>>;
  update(updatedItem: T): Promise<Result<T>>;
  delete(id: string): Promise<Result<void>>;
  getByIds(ids: string[]): Promise<Result<T[]>>;
}
