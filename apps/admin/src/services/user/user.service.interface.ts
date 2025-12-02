import type { Paginated, RecentActivity, Result, TargetType, UserPreference } from '@/types';
import type { UserProfile, UpdateUserProfileRequestDto } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserService {
  search(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserProfile[]; totalItems: number; totalPages: number; }, ApiError>>;
  getByIds(ids: string[]): Promise<Result<UserProfile[], ApiError>>;
  getById(id: string): Promise<Result<UserProfile, ApiError>>;
  getRecentActivities( // NEW METHOD
    page: number,
    itemsPerPage?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>>;
  getUserPreferences(): Promise<Result<UserPreference, ApiError>>; // NEW METHOD
  saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>>; // NEW METHOD
  updateUserProfile(profile: UpdateUserProfileRequestDto): Promise<Result<UserProfile, ApiError>>; // NEW METHOD
  getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>>; // NEW METHOD
}
