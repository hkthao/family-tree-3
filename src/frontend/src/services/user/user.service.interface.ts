import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserService {
  searchUsers(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserProfile[]; totalItems: number; totalPages: number; }, ApiError>>;
  getUsersByIds(ids: string[]): Promise<Result<UserProfile[], ApiError>>;
  getUserById(id: string): Promise<Result<UserProfile, ApiError>>;
}
