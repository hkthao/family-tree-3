// apps/mobile/family_tree_rn/services/userProfile/api.userProfile.service.ts

import { ApiClientMethods, UserProfileDto, Result, ApiError } from '@/types';
import { IUserProfileService } from '@/services/userProfile/userProfile.service.interface';

export class ApiUserProfileService implements IUserProfileService {
  constructor(private api: ApiClientMethods) {}

  async getCurrentUserProfile(): Promise<Result<UserProfileDto>> {
    try {
      const response = await this.api.get<UserProfileDto>('/api/user-profile/me');
      return { isSuccess: true, value: response };
    } catch (error: any) {
      // Basic error handling, can be expanded for more detail
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }
}
