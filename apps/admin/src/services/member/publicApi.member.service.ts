import {
  type Member,
  type Result,
  ok,
} from '@/types';
import type { IPublicMemberService } from './public.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

// Helper function to transform date strings to Date objects
function transformMemberDates(member: any): Member {
  if (member.dateOfBirth && typeof member.dateOfBirth === 'string') {
    member.dateOfBirth = new Date(member.dateOfBirth);
  }
  if (member.dateOfDeath && typeof member.dateOfDeath === 'string') {
    member.dateOfDeath = new Date(member.dateOfDeath);
  }
  return member;
}

export class PublicApiMemberService implements IPublicMemberService {
  constructor(private http: ApiClientMethods) {}

  async getPublicMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>> {
    const result = await this.http.get<Member[]>(`/public/family/${familyId}/members`);
    if (result.ok) {
      return ok(result.value.map((m) => transformMemberDates(m)));
    }
    return result;
  }

  async getPublicMemberById(id: string, familyId: string): Promise<Result<Member | undefined, ApiError>> {
    const result = await this.http.get<Member>(`/public/family/${familyId}/member/${id}?familyId=${familyId}`);
    if (result.ok) {
      return ok(result.value ? transformMemberDates(result.value) : undefined);
    }
    return result;
  }
}
