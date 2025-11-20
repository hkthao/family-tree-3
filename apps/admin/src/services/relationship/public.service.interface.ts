import type { Result } from '@/types/result';
import type { ApiError } from '@/types/api-error';
import type { Relationship } from '@/types';

export interface IPublicRelationshipService {
  getPublicRelationshipsByFamilyId(familyId: string): Promise<Result<Relationship[], ApiError>>;
}
