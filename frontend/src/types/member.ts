export interface Member {
  id: string;
  fullName: string;
  nickname?: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender: 'Male' | 'Female' | 'Other';
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  biography?: string;
  avatarUrl?: string;
  familyId: string;
  fatherId?: string;
  motherId?: string;
  spouseId?: string;
}

export interface MemberFilter {
  fullName?: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender?: 'Male' | 'Female' | 'Other';
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  familyId?: string;
}
