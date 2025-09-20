export interface Relationship {
  relatedMemberId: string;
  relationshipType: 'parent' | 'child' | 'spouse' | 'other'; // Or a more granular enum
  // Add other properties if needed, e.g., startDate, endDate, notes
}

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
  parents: Relationship[];
  spouses: Relationship[];
  children: Relationship[];
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
