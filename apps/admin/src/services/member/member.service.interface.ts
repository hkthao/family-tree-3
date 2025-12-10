import type { Member, Result, MemberFilter, Paginated } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberService extends ICrudService<Member> {
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; 
  addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>>; 
  updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>>; 
  getRelatives(memberId: string): Promise<Result<Member[], ApiError>>; 
}
