import type { Result } from '@/types/common/result';
import type { UserProfile } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IUserProfileService {
  getAllUserProfiles(): Promise<Result<UserProfile[], ApiError>>;
}
