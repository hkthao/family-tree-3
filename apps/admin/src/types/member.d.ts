import type { Relationship, LunarDate } from '@/types';

export enum Gender {
  Male = 'Male',
  Female = 'Female',
  Other = 'Other',
}

export interface MemberDto {
  id: string;
  lastName: string; // Last name
  firstName: string; // First name
  fullName?: string; // Full name (optional, often derived)
  code?: string; // New: Unique code for the member
  familyId: string;
  gender?: Gender;
  dateOfBirth?: Date; // Renamed from 'birthDate'
  lunarDateOfBirth?: LunarDate; // New: Lunar date of birth
  dateOfDeath?: Date; // Renamed from 'deathDate'
  lunarDateOfDeath?: LunarDate; // New: Lunar date of death
  birthDeathYears?: string; // Formatted birth and death years
  avatarUrl?: string;
  avatarBase64?: string | null;
  nickname?: string; // New
  placeOfBirth?: string; // New (replaces address)
  placeOfDeath?: string; // New (replaces address)
  birthLocationId?: string | null; // Added
  deathLocationId?: string | null; // Added
  residenceLocationId?: string | null; // Added
  phone?: string; // New
  email?: string; // New
  address?: string; // New
  occupation?: string; // New
  familyName?: string; // New
  familyAvatarUrl?: string; // New
  biography?: string; // New
  isRoot?: boolean;
  isDeceased?: boolean;
  validationErrors?: string[];
  relationships?: Relationship[]; // New: List of relationships for the member
  fatherId?: string; // New: ID of the father
  motherId?: string; // New: ID of the mother
  husbandId?: string; // New: ID of the husband
  wifeId?: string; // New: ID of the wife

  fatherFullName?: string;
  fatherAvatarUrl?: string;
  motherFullName?: string;
  motherAvatarUrl?: string;
  husbandFullName?: string;
  husbandAvatarUrl?: string;
  wifeFullName?: string;
  wifeAvatarUrl?: string;

  order?: number; // New: Order of the member in the family
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type MemberAddDto = Omit<
  MemberDto,
  | 'id'
  | 'relationships'
  | 'fullName'
  | 'birthDeathYears'
  | 'familyName'
  | 'familyAvatarUrl'
  | 'validationErrors'
  | 'fatherFullName'
  | 'fatherAvatarUrl'
  | 'motherFullName'
  | 'motherAvatarUrl'
  | 'husbandFullName'
  | 'husbandAvatarUrl'
  | 'wifeFullName'
  | 'wifeAvatarUrl'
>;

export type MemberUpdateDto = Omit<
  MemberDto,
  | 'relationships'
  | 'fullName'
  | 'birthDeathYears'
  | 'familyName'
  | 'familyAvatarUrl'
  | 'validationErrors'
  | 'fatherFullName'
  | 'fatherAvatarUrl'
  | 'motherFullName'
  | 'motherAvatarUrl'
  | 'husbandFullName'
  | 'husbandAvatarUrl'
  | 'wifeFullName'
  | 'wifeAvatarUrl'
>;

export interface MemberFilter {
  gender?: Gender | undefined;
  familyId?: string | null;
  fatherId?: string | null;
  motherId?: string | null;
  husbandId?: string | null;
  wifeId?: string | null;
  searchQuery?: string; // New property for search term
}