import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserService {
  search(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserProfile[]; totalItems: number; totalPages: number; }, ApiError>>;
  getByIds(ids: string[]): Promise<Result<UserProfile[], ApiError>>;
  getById(id: string): Promise<Result<UserProfile, ApiError>>;
}
