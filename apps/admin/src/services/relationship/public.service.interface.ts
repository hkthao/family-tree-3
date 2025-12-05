import type { ApiError } from '@/plugins/axios';
import type { Result, Relationship } from '@/types';

export interface IPublicRelationshipService {
  getPublicRelationshipsByFamilyId(familyId: string): Promise<Result<Relationship[], ApiError>>;
}
