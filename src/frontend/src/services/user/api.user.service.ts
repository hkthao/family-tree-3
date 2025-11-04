import type { IUserService } from './user.service.interface';
import type { UserProfile, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { ApiClient } from '@/plugins/axios';

export class ApiUserService implements IUserService {
  constructor(private apiClient: ApiClient) {}

  async searchUsers(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserProfile[]; totalItems: number; totalPages: number; }, ApiError>> {
    try {
      // Simulate API call to a new backend endpoint for searching users
      // This endpoint would handle filtering, pagination, and sorting on the server.
      const response = await this.apiClient.get('/api/users/search', {
        params: {
          searchQuery,
          page,
          itemsPerPage,
        },
      });
      return { ok: true, value: response.data };
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async getUsersByIds(ids: string[]): Promise<Result<UserProfile[], ApiError>> {
    try {
      const response = await this.apiClient.get('/api/users/by-ids', {
        params: { ids: ids.join(',') },
      });
      return { ok: true, value: response.data };
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }

  async getUserById(id: string): Promise<Result<UserProfile, ApiError>> {
    try {
      const response = await this.apiClient.get(`/api/users/${id}`);
      return { ok: true, value: response.data };
    } catch (error: any) {
      return { ok: false, error: error };
    }
  }
}
