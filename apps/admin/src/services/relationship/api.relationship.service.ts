import type { IRelationshipService } from './relationship.service.interface';
import type { ApiError, Relationship, RelationshipDetectionResult, Result } from '@/types';
import { type ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiRelationshipService extends ApiCrudService<Relationship, Relationship, Relationship> implements IRelationshipService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/relationship');
  }

  async getRelationShips(familyId: string): Promise<Result<Relationship[], ApiError>> {
    const payload = { familyId: familyId };
    return this.http.get<Relationship[]>(`/relationship`, {
      params: payload
    });
  }

  async addItems(newItems: Relationship[]): Promise<Result<string[], ApiError>> {
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
