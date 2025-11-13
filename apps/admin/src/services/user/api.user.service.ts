import type { IUserService } from './user.service.interface';
import type { UserProfile, Result, Paginated, User } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

export class ApiUserService implements IUserService {
  constructor(private http: ApiClientMethods) { }

  async search(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserProfile[]; totalItems: number; totalPages: number; }, ApiError>> {
    return await this.http.get<Paginated<User>>(`/user/search`, {
      params: {
        searchQuery,
        page,
        itemsPerPage,
      },
    });
  }

  async getByIds(ids: string[]): Promise<Result<User[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<User[]>(
      `/user/by-ids?${params.toString()}`,
    );
  }

  async getById(id: string): Promise<Result<User, ApiError>> {
    return this.http.get<User>(
      `/user/${id}`,
    );
  }
}
