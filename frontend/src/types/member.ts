export interface Member {
  id: string;
  fullName: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender: 'Male' | 'Female' | 'Other';
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  biography?: string;
  avatarUrl?: string;
  parents: string[]; // Array of member IDs
  spouses: string[]; // Array of member IDs
  children: string[]; // Array of member IDs
}

export interface MemberFilter {
  fullName?: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender?: 'Male' | 'Female' | 'Other';
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  relationship?: string; // e.g., 'parent', 'spouse', 'child'
}
