import { Theme, Language } from '@/types';

export interface UserPreference {
  id: string;
  userProfileId: string;
  theme: Theme;
  language: Language;
  emailNotificationsEnabled: boolean;
  smsNotificationsEnabled: boolean;
  inAppNotificationsEnabled: boolean;
  created: string;
  createdBy: string | null;
  lastModified: string | null;
  lastModifiedBy: string | null;
}