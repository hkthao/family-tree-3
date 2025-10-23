import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserProfileService {
  getAllUserProfiles(): Promise<Result<UserProfile[], ApiError>>;
  getUserProfile(id: string): Promise<Result<UserProfile, ApiError>>;
  getUserProfileByExternalId(externalId: string): Promise<Result<UserProfile, ApiError>>;
  updateUserProfile(profile: UserProfile): Promise<Result<UserProfile, ApiError>>;
  getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>>;
}
