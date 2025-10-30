import { Theme, Language } from '@/types';

export interface UserPreference {
  id: string;
  userProfileId: string;
  theme: Theme;
  language: Language;
  created: string;
  createdBy: string | null;
  lastModified: string | null;
  lastModifiedBy: string | null;
}