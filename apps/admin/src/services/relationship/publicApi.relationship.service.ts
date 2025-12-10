import { ok, type Result, type Relationship, type ApiError } from '@/types';
import type { ApiClientMethods } from '@/plugins/axios';
import type { IPublicRelationshipService } from './public.service.interface';

export class PublicApiRelationshipService implements IPublicRelationshipService {
  constructor(private http: ApiClientMethods) {}

  async getPublicRelationshipsByFamilyId(familyId: string): Promise<Result<Relationship[], ApiError>> {
    const result = await this.http.get<Relationship[]>(`/public/family/${familyId}/relationships`);
    if (result.ok) {
      return ok(result.value);
    }
    return result;
  }
}
