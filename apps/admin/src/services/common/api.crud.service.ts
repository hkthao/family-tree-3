import type { ICrudService } from './crud.service.interface';
import type { Result } from '@/types';
import type { PaginatedList, ListOptions, FilterOptions } from '@/types';
import type { ApiClientMethods } from '@/plugins/axios';

export class ApiCrudService<T extends { id?: string }> implements ICrudService<T> {
  constructor(protected api: ApiClientMethods, protected baseUrl: string) { }

  async search(
    options: ListOptions = { page: 1, itemsPerPage: 10, sortBy: [] },
    filters: FilterOptions = {},
  ): Promise<Result<PaginatedList<T>>> {
    const params: Record<string, any> = {
      page: options.page,
      pageSize: options.itemsPerPage,
    };
    if (options.sortBy && options.sortBy.length > 0) {
      params.orderBy = options.sortBy.map(s => `${s.key} ${s.order}`).join(',');
    }

    for (const key in filters) {
      if (filters[key] !== undefined) {
        params[key] = filters[key];
      }
    }
    return await this.api.get<PaginatedList<T>>(`${this.baseUrl}/search`, { params });
  }

  async getById(id: string): Promise<Result<T | undefined>> {
    return await this.api.get<T>(`${this.baseUrl}/${id}`);
  }

  async add(newItem: Omit<T, 'id'>): Promise<Result<T>> {
    return await this.api.post<T>(this.baseUrl, newItem);
  }

  async update(updatedItem: T): Promise<Result<T>> {
    return await this.api.put<T>(`${this.baseUrl}/${updatedItem.id as string}`, updatedItem);
  }

  async delete(id: string): Promise<Result<void>> {
    return await this.api.delete(`${this.baseUrl}/${id}`);
  }

  async getByIds(ids: string[]): Promise<Result<T[]>> {
    return await this.api.get<T[]>(`${this.baseUrl}/by-ids`, { params: { ids: ids.join(',') } });
  }
}