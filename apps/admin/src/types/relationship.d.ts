export enum RelationshipType {
  Father = 0,
  Mother = 1,
  Husband = 2,
  Wife = 3,
  Child = 4,
}

export interface RelationshipFilter {
  searchQuery?: string;
  sourceMemberId?: string | null;
  targetMemberId?: string | null;
  familyId?: string | null;
  type?: string | null;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  memberIds?: string[]; // New property for filtering relationships by a list of member IDs
}

export interface RelationshipMember {
  dateOfBirth?: Date;
  fullName: string;
  gender?: string;
  avatarUrl?: string;
}

export interface Relationship {
  id: string;
  sourceMemberName?: string;
  sourceMemberCode?: string;
  sourceMemberId: string;
  sourceMemberFullName?: string;
  targetMemberName?: string;
  targetMemberCode?: string;
  targetMemberId: string;
  targetMemberFullName?: string;
  type: RelationshipType;
  order?: number | null;
  startDate?: string;
  endDate?: string;
  description?: string;
  familyName?: string;
  familyCode?: string;
  familyId?: string;
  validationErrors?: string[];
  sourceMember?: RelationshipMember;
  targetMember?: RelationshipMember;
}
