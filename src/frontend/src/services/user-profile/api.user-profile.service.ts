import type { Result } from '@/types';
import type { UserProfile } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IUserProfileService } from './user-profile.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export class UserProfileApiService implements IUserProfileService {
  private apiUrl = `${API_BASE_URL}/user-profile`;

  constructor(private http: ApiClientMethods) {}

  public async getAllUserProfiles(): Promise<Result<UserProfile[], ApiError>> {
    return this.http.get<UserProfile[]>(this.apiUrl);
  }

  public async getUserProfile(id: string): Promise<Result<UserProfile, ApiError>> {
    return this.http.get<UserProfile>(`${this.apiUrl}/${id}`);
  }

  public async getUserProfileByExternalId(externalId: string): Promise<Result<UserProfile, ApiError>> {
    return this.http.get<UserProfile>(`${this.apiUrl}/byExternalId/${externalId}`);
  }

  public async updateUserProfile(profile: UserProfile): Promise<Result<UserProfile, ApiError>> {
    return this.http.put<UserProfile>(`${this.apiUrl}/${profile.id}`, profile);
  }

  public async getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>> {
    return this.http.get<UserProfile>(`${this.apiUrl}/me`);
  }
}
