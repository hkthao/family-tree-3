import type { RelationshipType } from './relationship-type';

export interface Relationship {
  id: string;
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType;
  order?: number;
}
