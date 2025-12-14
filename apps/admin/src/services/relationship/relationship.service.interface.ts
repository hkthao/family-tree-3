import type { Relationship, Result, RelationshipDetectionResult, ApiError } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IRelationshipService extends ICrudService<Relationship> {
  getRelationShips(familyId: string): Promise<Result<Relationship[], ApiError>>;
  addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>>; 
  detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<Result<RelationshipDetectionResult> | null>; 
}
