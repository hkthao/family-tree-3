// apps/mobile/family_tree_rn/services/userProfile/userProfile.service.interface.ts

import { UserProfileDto } from '@/types/userProfile';
import { Result } from '@/types/api';

export interface IUserProfileService {
  getCurrentUserProfile(): Promise<Result<UserProfileDto>>;
}
