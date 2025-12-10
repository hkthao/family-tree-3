import type { Result, Relationship, ApiError } from '@/types';

export interface IPublicRelationshipService {
  getPublicRelationshipsByFamilyId(familyId: string): Promise<Result<Relationship[], ApiError>>;
}
