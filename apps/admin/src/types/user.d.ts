import { Theme } from '@/types';

export enum Language {
  English = 0,
  Vietnamese = 1,
}

export interface User {
  id: string;
  externalId: string;
  name: string;
  email: string;
  roles?: string[];
  avatar?: string;
  online?: boolean;
}

export interface UserMenuItem {
  key: string;
  labelKey: string;
  icon: string;
  to?: string;
}

export interface UserProfile {
    id: string;
    externalId: string;
    userId?: string;
    email: string;
    name: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    avatar?: string;
    roles?: string[];
}

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

export interface UpdateUserProfileRequestDto {
    id: string;
    externalId?: string;
    email: string;
    name: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    avatarBase64?: string | null; // Can be a base64 string, null (to clear), or undefined (no change)
}