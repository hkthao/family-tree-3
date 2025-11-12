import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface ICurrentUserProfileService {
  updateUserProfile(profile: UserProfile): Promise<Result<UserProfile, ApiError>>;
  getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>>;
}
