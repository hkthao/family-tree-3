// apps/mobile/family_tree_rn/services/relationship/relationship.service.interface.ts

import { Result } from '@/types';
import { RelationshipListDto } from '@/types';

export interface IRelationshipService {
  getRelationshipsByFamilyId(familyId: string): Promise<Result<RelationshipListDto[]>>;
}
