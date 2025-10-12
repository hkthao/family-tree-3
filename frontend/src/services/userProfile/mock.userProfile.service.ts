import type { Result } from '@/types/common/result';
import type { UserProfile } from '@/types';
import type { IUserProfileService } from './userProfile.service.interface';
import { ok } from '@/types/common/result';
import type { ApiError } from '@/plugins/axios';

export class MockUserProfileService implements IUserProfileService {
  getCurrentUserProfile(): Promise<Result<UserProfile, ApiError>> {
    throw new Error('Method not implemented.');
  }
  private userProfiles: UserProfile[] = [
    { id: '1', externalId: 'auth0|1', email: 'john.doe@example.com', name: 'John Doe' },
    { id: '2', externalId: 'auth0|2', email: 'jane.smith@example.com', name: 'Jane Smith' },
    { id: '3', externalId: 'auth0|3', email: 'peter.jones@example.com', name: 'Peter Jones' },
  ];

  async getAllUserProfiles(): Promise<Result<UserProfile[]>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    return ok(this.userProfiles);
  }

  async getUserProfile(id: string): Promise<Result<UserProfile>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    const profile = this.userProfiles.find(p => p.id === id);
    if (profile) {
      return ok(profile);
    } else {
      return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
    }
  }

  async getUserProfileByExternalId(externalId: string): Promise<Result<UserProfile>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    const profile = this.userProfiles.find(p => p.externalId === externalId);
    if (profile) {
      return ok(profile);
    } else {
      return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
    }
  }

  async updateUserProfile(profile: UserProfile): Promise<Result<UserProfile>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    const index = this.userProfiles.findIndex(p => p.id === profile.id);
    if (index !== -1) {
      this.userProfiles[index] = profile;
      return ok(profile);
    } else {
      return { ok: false, error: { message: 'Profile not found', statusCode: 404 } as ApiError };
    }
  }
}
