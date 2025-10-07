import type { Result } from '@/types/common/result';
import type { UserProfile } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IUserProfileService } from './userProfile.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class UserProfileApiService implements IUserProfileService {
  private apiUrl = `${API_BASE_URL}/UserProfiles`;

  constructor(private http: ApiClientMethods) {}

  public async getAllUserProfiles(): Promise<Result<UserProfile[], ApiError>> {
    return this.http.get<UserProfile[]>(this.apiUrl);
  }
}
