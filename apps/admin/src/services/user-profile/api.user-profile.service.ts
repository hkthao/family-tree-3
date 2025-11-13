import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { ICurrentUserProfileService } from './user-profile.service.interface';

export class UserProfileApiService implements ICurrentUserProfileService {
  constructor(private http: ApiClientMethods) {}

  public async updateUserProfile(profile: UserProfile): Promise<Result<UserProfile, ApiError>> {
    return this.http.put<UserProfile>(`/user-profile/${profile.id}`, profile);
  }

  public async getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>> {
    return this.http.get<UserProfile>(`/user-profile/me`);
  }
}
