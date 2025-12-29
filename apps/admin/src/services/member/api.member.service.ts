import {
  type MemberDto,
  type MemberAddDto,
  type MemberUpdateDto,
} from '@/types';
import type { ApiError, Result } from '@/types';
import type { IMemberService } from './member.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';


function prepareMemberForApi(member: MemberAddDto | MemberUpdateDto): any {
  const apiMember: any = { ...member };

  if (apiMember.dateOfBirth instanceof Date) {
    apiMember.dateOfBirth = apiMember.dateOfBirth.toISOString();
  }
  if (apiMember.dateOfDeath instanceof Date) {
    apiMember.dateOfDeath = apiMember.dateOfDeath.toISOString();
  }
  return apiMember;
}

export class ApiMemberService extends ApiCrudService<MemberDto, MemberAddDto, MemberUpdateDto> implements IMemberService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/member');
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<MemberDto[], ApiError>> {
    return await this.http.get<MemberDto[]>(
      `/member/by-family/${familyId}`,
    );
  }

  async getById(id: string): Promise<Result<MemberDto | undefined, ApiError>> {

    return await this.http.get<MemberDto>(`/member/${id}`);
  }

  async add(newItem: MemberAddDto): Promise<Result<MemberDto, ApiError>> {
    const apiMember = prepareMemberForApi(newItem);
    return await this.http.post<MemberDto>(`/member`, apiMember);
  }

  async addItems(newItems: MemberAddDto[]): Promise<Result<string[], ApiError>> {
    const apiMembers = newItems.map(prepareMemberForApi);
    return this.http.post<string[]>(`/member/bulk-create`, { members: apiMembers });
  }

  async update(updatedItem: MemberUpdateDto): Promise<Result<MemberDto, ApiError>> {
    const apiMember = prepareMemberForApi(updatedItem);
    return await this.http.put<MemberDto>(
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

  async getRelatives(memberId: string): Promise<Result<MemberDto[], ApiError>> {
    return await this.http.get<MemberDto[]>(`/member/${memberId}/relatives`);
  }

  async exportMembers(familyId?: string): Promise<Result<string, ApiError>> {
    const url = `${this.baseUrl}/export`;
    return this.http.get<string>(url, { params: { familyId } });
  }

  async importMembers(familyId: string, payload: any): Promise<Result<void, ApiError>> {
    const url = `${this.baseUrl}/import`;
    return this.http.post<void>(url, payload, { params: { familyId } });
  }
}
