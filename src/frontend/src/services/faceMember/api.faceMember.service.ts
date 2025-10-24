import type { Result, Member } from '@/types';
import type { IFaceMemberService } from './faceMember.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';

export class ApiFaceMemberService implements IFaceMemberService {
  private client: ApiClientMethods;

  constructor(client: ApiClientMethods) {
    this.client = client;
  }

  async getManagedMembers(): Promise<Result<Member[], Error>> {
    try {
      const response = await this.client.get<Member[]>('/api/member/managed');
      if (response.ok) {
        return { ok: true, value: response.value || [] };
      } else {
        return { ok: false, error: response.error || new Error('Failed to fetch managed members') };
      }
    } catch (error: any) {
      return { ok: false, error: error || new Error('An unexpected error occurred') };
    }
  }
}
