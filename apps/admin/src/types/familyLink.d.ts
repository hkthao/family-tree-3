export enum LinkStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

export interface FamilyLinkRequestDto  {
  id: string;
  requestingFamilyId: string;
  requestingFamilyName: string;
  targetFamilyId: string;
  targetFamilyName: string;
  status: LinkStatus;
  requestDate: string;
  responseDate?: string;
}

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

export interface FamilyLinkRequestFilter {
  familyId?: string;
  searchQuery?: string;
  status?: LinkStatus; // Re-introduce status for FamilyLinkRequest filtering
  otherFamilyId?: string | null;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface UpdateFamilyLinkRequestCommand {
  id: string;
  status: LinkStatus;
}