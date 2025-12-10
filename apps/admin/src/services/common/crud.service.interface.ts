import type { Result, PaginatedList, ListOptions, FilterOptions } from '@/types';
import type { ApiError } from '@/types/api.error'; // Changed from '@/plugins/axios'

export interface ICrudService<T> {
  search(
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<PaginatedList<T>>>;
  getById(id: string): Promise<Result<T | undefined>>; // Removed ApiError here, assuming Result already handles it
  add(newItem: Omit<T, 'id'>): Promise<Result<T>>; // Removed ApiError here
  update(updatedItem: T): Promise<Result<T>>; // Removed ApiError here
  delete(id: string): Promise<Result<void>>; // Removed ApiError here
  getByIds(ids: string[]): Promise<Result<T[]>>; // Removed ApiError here
}
