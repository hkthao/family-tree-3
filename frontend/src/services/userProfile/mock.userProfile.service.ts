import type { Result } from '@/types/common/result';
import type { UserProfile } from '@/types';
import type { IUserProfileService } from './userProfile.service.interface';
import { ok } from '@/types/common/result';

export class MockUserProfileService implements IUserProfileService {
  private userProfiles: UserProfile[] = [
    { id: '1', auth0UserId: 'auth0|1', email: 'john.doe@example.com', name: 'John Doe' },
    { id: '2', auth0UserId: 'auth0|2', email: 'jane.smith@example.com', name: 'Jane Smith' },
    { id: '3', auth0UserId: 'auth0|3', email: 'peter.jones@example.com', name: 'Peter Jones' },
  ];

  async getAllUserProfiles(): Promise<Result<UserProfile[]>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    return ok(this.userProfiles);
  }
}
