import type { ICrudService } from './crud.service.interface';
import type { Result } from '@/types';
import type { Paginated, ListOptions, FilterOptions } from '@/types';
import type { ApiClientMethods } from '@/plugins/axios';
import { buildSearchParams } from '@/utils/list.utils'; // Import the utility function

export class ApiCrudService<TGet extends { id?: string }, TAdd, TUpdate extends { id?: string }> implements ICrudService<TGet, TAdd, TUpdate> {
  constructor(protected api: ApiClientMethods, protected baseUrl: string) { }

  async search(
    options: ListOptions = { page: 1, itemsPerPage: 10, sortBy: [] },
    filters: FilterOptions = {},
  ): Promise<Result<Paginated<TGet>>> {
    const params = buildSearchParams(options, filters); // Use the utility function
    return await this.api.get<Paginated<TGet>>(`${this.baseUrl}/search`, { params });
  }

  async getById(id: string): Promise<Result<TGet | undefined>> {
    return await this.api.get<TGet>(`${this.baseUrl}/${id}`);
  }

  async add(newItem: TAdd): Promise<Result<TGet>> {
    return await this.api.post<TGet>(this.baseUrl, newItem);
  }

  async update(updatedItem: TUpdate): Promise<Result<TGet>> {
    return await this.api.put<TGet>(`${this.baseUrl}/${updatedItem.id as string}`, updatedItem);
  }

  async delete(id: string): Promise<Result<void>> {
    return await this.api.delete(`${this.baseUrl}/${id}`);
  }

  async getByIds(ids: string[]): Promise<Result<TGet[]>> {
    return await this.api.get<TGet[]>(`${this.baseUrl}/by-ids`, { params: { ids: ids.join(',') } });
  }
}