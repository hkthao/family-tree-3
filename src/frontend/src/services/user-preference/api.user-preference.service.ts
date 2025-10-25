import type { IUserPreferenceService } from './user-preference.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, UserPreference } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiUserPreferenceService implements IUserPreferenceService {
  private apiUrl = `${API_BASE_URL}/UserPreferences`;

  constructor(private http: ApiClientMethods) {}

  async getUserPreferences(): Promise<Result<UserPreference, ApiError>> {
    return this.http.get<UserPreference>(this.apiUrl);
  }

  async saveUserPreferences(preferences: UserPreference): Promise<Result<void, ApiError>> {
    return this.http.put<void>(this.apiUrl, preferences);
  }
}
