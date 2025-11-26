import type { IUserService } from './user.service.interface';
import type { UserProfile, Result, Paginated, User, RecentActivity, TargetType, UserPreference } from '@/types'; // NEW IMPORTS
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

  async getRecentActivities( // NEW METHOD
    page: number,
    itemsPerPage?: number,
    targetType?: TargetType,
    targetId?: string,
    groupId?: string,
  ): Promise<Result<Paginated<RecentActivity>, ApiError>> {
    const params = new URLSearchParams();
    if (page) params.append('page', page.toString())
    if (itemsPerPage) params.append('itemsPerPage', itemsPerPage.toString());
    if (targetType !== undefined) params.append('targetType', targetType.toString());
    if (targetId) params.append('targetId', targetId);
    if (groupId) params.append('groupId', groupId);
    return this.http.get<Paginated<RecentActivity>>(`/activity/recent?${params.toString()}`);
  }

  async getUserPreferences(): Promise<Result<UserPreference, ApiError>> { // NEW METHOD
    return this.http.get<UserPreference>(`/user-preference`);
  }

  async saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>> { // NEW METHOD
    return this.http.put<void>(`/user-preference`, preferences);
  }

  public async updateUserProfile(profile: UserProfile): Promise<Result<UserProfile, ApiError>> { // NEW METHOD
    return this.http.put<UserProfile>(`/user-profile/${profile.id}`, profile);
  }

  public async getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>> { // NEW METHOD
    return this.http.get<UserProfile>(`/user-profile/me`);
  }
}
