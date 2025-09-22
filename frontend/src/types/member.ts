export interface Member {
  id: string;
  fullName: string; // Renamed from 'name'
  familyId: string;
  gender?: 'male' | 'female' | 'other';
  dateOfBirth?: Date; // Renamed from 'birthDate'
  dateOfDeath?: Date; // Renamed from 'deathDate'
  avatarUrl?: string;
  nickname?: string; // New
  placeOfBirth?: string; // New (replaces address)
  placeOfDeath?: string; // New (replaces address)
  occupation?: string; // New
  fatherId?: string; // New
  motherId?: string; // New
  spouseId?: string; // New
  biography?: string; // New
}
