import type { RelationshipType } from '@/types/common';

export interface Relationship {
  id?: string;
  sourceMemberName?: string;
  sourceMemberCode?: string;
  sourceMemberId: string;
  sourceMemberFullName?: string;
  targetMemberName?: string;
  targetMemberCode?: string;
  targetMemberId: string;
  targetMemberFullName?: string; // Added
  type: RelationshipType;
  order?: number;
  startDate?: string;
  endDate?: string;
  description?: string;
  familyName?: string;
  familyCode?: string;
  familyId?: string;
  validationErrors?: string[];
}

export interface CreateRelationshipPayload {
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType;
  order?: number;
  startDate?: string;
  endDate?: string;
  description?: string;
}