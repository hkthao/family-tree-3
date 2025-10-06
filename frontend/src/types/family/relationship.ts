import type { RelationshipType } from './relationship-type';

export interface RelationshipMember {
  dateOfBirth?: Date;
  fullName: string;
  gender?: string;
  avatarUrl?: string;
}

export interface Relationship {
  id: string;
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType;
  order?: number;
  sourceMember?: RelationshipMember;
  targetMember?: RelationshipMember;
}
