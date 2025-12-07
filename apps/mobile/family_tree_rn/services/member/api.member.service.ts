// apps/mobile/family_tree_rn/services/member/api.member.service.ts

import { ApiClientMethods } from '@/types';
import { Result, ApiError, PaginatedList, MemberListDto, SearchPublicMembersQuery, MemberDetailDto } from '@/types';
import { IMemberService } from '@/services/member/member.service.interface';

export class ApiMemberService implements IMemberService {
  constructor(private api: ApiClientMethods) {}

  async searchMembers(query: SearchPublicMembersQuery): Promise<Result<PaginatedList<MemberListDto>>> {
    try {
      const response = await this.api.get<PaginatedList<MemberListDto>>('/members/search', {
        params: query,
      });
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }

  async getMembersByFamilyId(familyId: string): Promise<Result<MemberListDto[]>> {
    try {
      const response = await this.api.get<MemberListDto[]>(`/family/${familyId}/members`);
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }

  async getMemberById(id: string, familyId: string): Promise<Result<MemberDetailDto>> {
    try {
      const response = await this.api.get<MemberDetailDto>(`/family/${familyId}/member/${id}`, {
        params: { familyId },
      });
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }
}
