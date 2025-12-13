import {
  type ApiError,
  type Member,
  type Result,
} from '@/types';
import type { IPublicMemberService } from './public.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';

export class PublicApiMemberService implements IPublicMemberService {
  constructor(private http: ApiClientMethods) { }

  async getPublicMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>> {
    return await this.http.get<Member[]>(`/public/family/${familyId}/members`);
  }

  async getPublicMemberById(id: string, familyId: string): Promise<Result<Member | undefined, ApiError>> {
    return await this.http.get<Member>(`/public/family/${familyId}/member/${id}?familyId=${familyId}`);
  }
}
