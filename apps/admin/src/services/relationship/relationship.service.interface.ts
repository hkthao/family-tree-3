import type { Relationship, Result, RelationshipDetectionResult, ApiError } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IRelationshipService extends ICrudService<Relationship, Relationship, Relationship> {
  getRelationShips(familyId: string): Promise<Result<Relationship[], ApiError>>;
  addItems(newItems: Relationship[]): Promise<Result<string[], ApiError>>;
  detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<Result<RelationshipDetectionResult> | null>;
}
