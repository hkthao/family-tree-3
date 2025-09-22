export interface Member {
  id: string;
  lastName: string; // Last name
  firstName: string; // First name
  fullName: string; // Full name
  familyId: string;
  gender?: 'male' | 'female' | 'other';
  dateOfBirth?: Date; // Renamed from 'birthDate'
  dateOfDeath?: Date; // Renamed from 'deathDate'
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
