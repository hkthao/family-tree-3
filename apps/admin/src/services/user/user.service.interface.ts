import type { ApiError, Paginated, RecentActivity, Result, TargetType, UpdateUserProfileDto, UserDto, UserPreference, UserProfile } from '@/types';

export interface IUserService {
  search(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<{ items: UserDto[]; totalItems: number; totalPages: number; }, ApiError>>;
  getByIds(ids: string[]): Promise<Result<UserDto[], ApiError>>;
  getById(id: string): Promise<Result<UserDto, ApiError>>;
  getRecentActivities( // NEW METHOD
    page: number,
    itemsPerPage?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>>;
  getUserPreferences(): Promise<Result<UserPreference, ApiError>>; // NEW METHOD
  saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>>; // NEW METHOD
  updateUserProfile(profile: UpdateUserProfileDto): Promise<Result<UserProfile, ApiError>>; // NEW METHOD
  getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>>; // NEW METHOD
}
