// apps/admin/src/types/public-relationship.d.ts
import type { BaseAuditableDto } from './common-dto';
import type { RelationshipType } from './relationship'; // Reusing RelationshipType from admin relationship types

export interface RelationshipMemberDto {
  id: string;
  fullName?: string;
  isRoot: boolean;
  avatarUrl?: string;
  dateOfBirth?: string;
}

export interface RelationshipDto {
  id: string;
  sourceMemberId: string;
  sourceMember?: RelationshipMemberDto;
  targetMemberId: string;
  targetMember?: RelationshipMemberDto;
  type: RelationshipType;
  order?: number;
  familyId: string;
}

export interface RelationshipListDto {
  id: string;
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType;
  order?: number;
  startDate?: string;
  endDate?: string;
  description?: string;
  sourceMember?: RelationshipMemberDto;
  targetMember?: RelationshipMemberDto;
}
