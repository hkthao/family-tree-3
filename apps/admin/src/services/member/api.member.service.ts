import {
  type Member,
} from '@/types';
import type { ApiError, Result } from '@/types';
import type { IMemberService } from './member.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';


function prepareMemberForApi(member: Omit<Member, 'id'> | Member): any {
  const apiMember: any = { ...member };

  if (apiMember.dateOfBirth instanceof Date) {
    apiMember.dateOfBirth = apiMember.dateOfBirth.toISOString();
  }
  if (apiMember.dateOfDeath instanceof Date) {
    apiMember.dateOfDeath = apiMember.dateOfDeath.toISOString();
  }
  return apiMember;
}

export class ApiMemberService extends ApiCrudService<Member> implements IMemberService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/member');
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    return await this.http.get<Member[]>(
      `/member/by-family/${familyId}`,
    );
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> {

    return await this.http.get<Member>(`/member/${id}`);
  }

  async add(newItem: Member): Promise<Result<Member, ApiError>> {
    const apiMember = prepareMemberForApi(newItem);
    return await this.http.post<Member>(`/member`, apiMember);
  }

  async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
    const apiMembers = newItems.map(prepareMemberForApi);
    return this.http.post<string[]>(`/member/bulk-create`, { members: apiMembers });
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    const apiMember = prepareMemberForApi(updatedItem);
    return await this.http.put<Member>(
      `/member/${updatedItem.id}`,
      apiMember,
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/member/${id}`);
  }

  async updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>> {
    const payload = { memberId, biographyContent };
    return this.http.put<void>(`/member/${memberId}/biography`, payload);
  }

  async getRelatives(memberId: string): Promise<Result<Member[], ApiError>> {
    return await this.http.get<Member[]>(`/member/${memberId}/relatives`);
  }
}
