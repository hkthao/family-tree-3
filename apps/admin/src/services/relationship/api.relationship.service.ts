import type { IRelationshipService } from './relationship.service.interface';
import type { Relationship, RelationshipDetectionResult } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result } from '@/types'; 
import { ApiCrudService } from '../common/api.crud.service';

export class ApiRelationshipService extends ApiCrudService<Relationship> implements IRelationshipService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/relationship');
  }

  async addItems(newItems: Omit<Relationship, 'id'>[]): Promise<Result<string[], ApiError>> {
    const payload = { relationships: newItems };
    return this.http.post<string[]>(`/relationship/bulk-create`, payload);
  }

  async detectRelationship(familyId: string, memberAId: string, memberBId: string): Promise<Result<RelationshipDetectionResult> | null> {
    return await this.http.get<RelationshipDetectionResult>(
        `/relationship/detect-relationship`,
        {
          params: {
            familyId,
            memberAId,
            memberBId,
          },
        },
      );
  }
}
