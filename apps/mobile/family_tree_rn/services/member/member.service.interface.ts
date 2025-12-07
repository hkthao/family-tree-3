// apps/mobile/family_tree_rn/services/member/member.service.interface.ts

import { Result } from '@/types';
import { PaginatedList, MemberListDto, SearchPublicMembersQuery, MemberDetailDto } from '@/types';

export interface IMemberService {
  searchMembers(query: SearchPublicMembersQuery): Promise<Result<PaginatedList<MemberListDto>>>;
  getMembersByFamilyId(familyId: string): Promise<Result<MemberListDto[]>>;
  getMemberById(id: string, familyId: string): Promise<Result<MemberDetailDto>>;
}
