import type { Relationship, Result, RelationshipDetectionResult } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IRelationshipService extends ICrudService<Relationship> {
  addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>>; 
  detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<Result<RelationshipDetectionResult> | null>; 
}
