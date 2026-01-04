



export interface FamilyLinkDto {
  id: string;
  family1Id: string;
  family1Name: string;
  family2Id: string;
  family2Name: string;
  linkDate: string;
}

export interface FamilyLinkFilter {
  familyId?: string;
  searchQuery?: string;
  otherFamilyId?: string | null;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
