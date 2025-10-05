export interface RelationshipFilter {
  sourceMemberId?: string | null;
  targetMemberId?: string | null;
  type?: string | null;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
