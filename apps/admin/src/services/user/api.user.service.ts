import type { IUserService } from './user.service.interface';
import type { UserDto, Result, Paginated, RecentActivity, TargetType, UserPreference, UpdateUserProfileDto, ApiError, UserProfile } from '@/types'; // NEW IMPORTS
import { type ApiClientMethods } from '@/plugins/axios';

export class ApiUserService implements IUserService {
  constructor(private http: ApiClientMethods) { }

  async search(searchQuery: string, page: number, itemsPerPage: number): Promise<Result<Paginated<UserDto>, ApiError>> {
    return await this.http.get<Paginated<UserDto>>(`/user/search`, {
      params: {
        searchQuery,
        page,
        itemsPerPage,
      },
    });
  }

  async getByIds(ids: string[]): Promise<Result<UserDto[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<UserDto[]>(
      `/user/by-ids?${params.toString()}`,
    );
  }

  async getById(id: string): Promise<Result<UserDto, ApiError>> {
    return this.http.get<UserDto>(
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

  public async updateUserProfile(profile: UpdateUserProfileDto): Promise<Result<UserProfile, ApiError>> { // NEW METHOD
    return this.http.put<UserProfile>(`/user-profile/${profile.id}`, profile);
  }

  public async getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>> { // NEW METHOD
    return this.http.get<UserProfile>(`/user-profile/me`);
  }

  public async findUser(usernameOrEmail: string): Promise<Result<UserDto, ApiError>> { // NEW METHOD
    return this.http.get<UserDto>(`/user/find?usernameOrEmail=${usernameOrEmail}`);
  }
}
