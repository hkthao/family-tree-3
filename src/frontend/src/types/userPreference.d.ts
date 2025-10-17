import { Theme, Language } from '@/types';

export interface UserPreference {
  userProfileId: string;
  theme: Theme;
  language: Language;
  emailNotificationsEnabled: boolean;
  smsNotificationsEnabled: boolean;
  inAppNotificationsEnabled: boolean;
}