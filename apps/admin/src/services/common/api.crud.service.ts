import type { AxiosInstance } from 'axios';
import type { ICrudService } from './crud.service.interface';
import type { Result } from '@/types/result';
import { ApiError } from '@/types/api.error';
import i18n from '@/plugins/i18n';
import type { PaginatedList, ListOptions, FilterOptions } from '@/types';

export class ApiCrudService<T extends { id: string }> implements ICrudService<T> {
  constructor(protected api: AxiosInstance, protected baseUrl: string) {}

  async search(
    options: ListOptions = { page: 1, itemsPerPage: 10, sortBy: [] },
    filters: FilterOptions = {},
  ): Promise<Result<PaginatedList<T>>> {
    try {
      const params: Record<string, any> = {
        page: options.page,
        pageSize: options.itemsPerPage,
      };
      if (options.sortBy && options.sortBy.length > 0) {
        params.orderBy = options.sortBy.map(s => `${s.key} ${s.order}`).join(',');
      }
      
      // Merge filters into params, excluding undefined values
      for (const key in filters) {
        if (filters[key] !== undefined) {
          params[key] = filters[key];
        }
      }

      const response = await this.api.get<PaginatedList<T>>(this.baseUrl, { params });
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.loadError')));
    }
  }

  async getById(id: string): Promise<Result<T | undefined>> {
    try {
      const response = await this.api.get<T>(`${this.baseUrl}/${id}`);
      return Result.success(response.data);
    } catch (error: any) {
      if (error.response && error.response.status === 404) {
        return Result.success(undefined);
      }
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.loadError')));
    }
  }

  async add(newItem: Omit<T, 'id'>): Promise<Result<T>> {
    try {
      const response = await this.api.post<T>(this.baseUrl, newItem);
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.addError')));
    }
  }

  async update(updatedItem: T): Promise<Result<T>> {
    try {
      const response = await this.api.put<T>(`${this.baseUrl}/${updatedItem.id}`, updatedItem);
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.updateError')));
    }
  }

  async delete(id: string): Promise<Result<void>> {
    try {
      await this.api.delete(`${this.baseUrl}/${id}`);
      return Result.success(undefined);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.deleteError')));
    }
  }

  async getByIds(ids: string[]): Promise<Result<T[]>> {
    try {
      const response = await this.api.get<T[]>(`${this.baseUrl}/batch`, { params: { ids: ids.join(',') } });
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('common.loadError')));
    }
  }
}
