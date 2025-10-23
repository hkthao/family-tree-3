import type { RelationshipType } from '@/types';


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
  order?: number;
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