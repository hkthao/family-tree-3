import type { Paginated, ListOptions, FilterOptions } from '@/types';
import type { Result } from '@/types'; // Explicitly import Result type

export interface ICrudService<TGet, TAdd, TUpdate> {
  search(
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<Paginated<TGet>>>;
  getById(id: string): Promise<Result<TGet | undefined>>;
  add(newItem: TAdd): Promise<Result<TGet>>;
  update(updatedItem: TUpdate): Promise<Result<TGet>>;
  delete(id: string): Promise<Result<void>>;
  getByIds(ids: string[]): Promise<Result<TGet[]>>;
}
