import type { ICurrentUserPreferenceService } from './user-preference.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, UserPreference } from '@/types';

export class ApICurrentUserPreferenceService implements ICurrentUserPreferenceService {
  constructor(private http: ApiClientMethods) {}

  async getUserPreferences(): Promise<Result<UserPreference, ApiError>> {
    return this.http.get<UserPreference>(`/user-preference`);
  }

  async saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>> {
    return this.http.put<void>(`/user-preference`, preferences);
  }
}
