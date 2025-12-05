import { ok, type Result, type Relationship } from '@/types';
import type { ApiError } from '@/plugins/axios';
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
