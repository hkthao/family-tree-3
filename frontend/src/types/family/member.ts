import { Gender } from '@/types';

export interface Member {
  id: string;
  lastName: string; // Last name
  firstName: string; // First name
  fullName?: string; // Full name (optional, often derived)
  familyId: string;
  gender?: Gender;
  dateOfBirth?: Date; // Renamed from 'birthDate'
  dateOfDeath?: Date; // Renamed from 'deathDate'
  birthDeathYears?: string; // Formatted birth and death years
  avatarUrl?: string;
  nickname?: string; // New
  placeOfBirth?: string; // New (replaces address)
  placeOfDeath?: string; // New (replaces address)
  occupation?: string; // New
  fatherId?: string | null;
  motherId?: string | null;
  spouseId?: string | null; // New
  biography?: string; // New
}

export interface MemberFilter {
  gender?: Gender | undefined;
  familyId?: string | null;
  searchQuery?: string; // New property for search term
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}
