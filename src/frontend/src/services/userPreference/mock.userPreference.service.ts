import type { IUserPreferenceService } from './userPreference.service.interface';
import type { Result, UserPreference } from '@/types';
import { ok } from '@/types';
import { simulateLatency } from '@/utils/mockUtils';
import { Theme, Language } from '@/types';

export class MockUserPreferenceService implements IUserPreferenceService {
  private mockPreferences: UserPreference = {
    id: 'mock-user-preference-id',
    userProfileId: 'a1b2c3d4-e5f6-7890-1234-567890abcdef',
    theme: Theme.Light,
    language: Language.English,
    emailNotificationsEnabled: true,
    smsNotificationsEnabled: false,
    inAppNotificationsEnabled: true,
    created: new Date().toISOString(),
    createdBy: 'mock-user',
    lastModified: new Date().toISOString(),
    lastModifiedBy: 'mock-user',
  };

  async getUserPreferences(): Promise<Result<UserPreference>> {
    console.log('Fetching mock user preferences');
    return simulateLatency(ok(this.mockPreferences));
  }

  async saveUserPreferences(preferences: UserPreference): Promise<Result<void>> {
    console.log('Saving mock user preferences:', preferences);
    this.mockPreferences = { ...preferences };
    return simulateLatency(ok(undefined));
  }
}
