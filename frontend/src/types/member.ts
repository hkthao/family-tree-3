export interface Member {
  id: string;
  fullName: string; // Renamed from 'name'
  familyId: string;
  gender?: 'male' | 'female' | 'other';
  dateOfBirth?: string; // Renamed from 'birthDate'
  dateOfDeath?: string; // Renamed from 'deathDate'
  avatarUrl?: string;
  nickname?: string; // New
  placeOfBirth?: string; // New (replaces address)
  placeOfDeath?: string; // New (replaces address)
  occupation?: string; // New
  fatherId?: string; // New
  motherId?: string; // New
  spouseId?: string; // New
  biography?: string; // New
  visibility?: 'public' | 'private' | 'shared';
}
