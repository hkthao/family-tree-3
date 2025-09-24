import type { Result } from '@/types/result';
import type { ApiError } from '@/utils/api';

export interface ICrudService<T> {
  fetch(): Promise<Result<T[], ApiError>>;
  getById(id: string): Promise<Result<T | undefined, ApiError>>;
  add(newItem: Omit<T, 'id'>): Promise<Result<T, ApiError>>;
  update(updatedItem: T): Promise<Result<T, ApiError>>;
  delete(id: string): Promise<Result<void, ApiError>>;
}
