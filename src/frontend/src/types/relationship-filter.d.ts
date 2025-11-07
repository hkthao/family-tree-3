export interface RelationshipFilter {
  searchQuery?: string;
  sourceMemberId?: string | null;
  targetMemberId?: string | null;
  familyId?: string | null;
  type?: string | null;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
