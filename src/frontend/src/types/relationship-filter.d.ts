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
