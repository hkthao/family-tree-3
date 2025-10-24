import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { UserPreference } from '@/types';

export interface IUserPreferenceService {
  getUserPreferences(): Promise<Result<UserPreference, ApiError>>;
  saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>>;
}
