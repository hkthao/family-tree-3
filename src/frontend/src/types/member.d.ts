import type { Relationship } from './relationship';

export enum Gender {
  Male = 'Male',
  Female = 'Female',
  Other = 'Other',
}

export interface Member {
  id: string;
  lastName: string; // Last name
  firstName: string; // First name
  fullName?: string; // Full name (optional, often derived)
  code?: string; // New: Unique code for the member
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
  familyName?: string; // New
  biography?: string; // New
  isRoot?: boolean;
  validationErrors?: string[];
  relationships?: Relationship[]; // New: List of relationships for the member
  fatherId?: string; // New: ID of the father
  motherId?: string; // New: ID of the mother
  husbandId?: string; // New: ID of the husband
  wifeId?: string; // New: ID of the wife
}

export interface MemberFilter {
  gender?: Gender | undefined;
  familyId?: string | null;
  searchQuery?: string; // New property for search term
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}