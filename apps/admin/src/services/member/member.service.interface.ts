import type { MemberDto, MemberAddDto, MemberUpdateDto, Result, ApiError } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberService extends ICrudService<MemberDto, MemberAddDto, MemberUpdateDto> {
  fetchMembersByFamilyId(familyId: string): Promise<Result<MemberDto[], ApiError>>;
  addItems(newItems: MemberAddDto[]): Promise<Result<string[], ApiError>>;
  updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>>;
  getRelatives(memberId: string): Promise<Result<MemberDto[], ApiError>>;
  exportMembers(familyId?: string): Promise<Result<string, ApiError>>;
  importMembers(familyId: string, payload: any): Promise<Result<void, ApiError>>;
}
