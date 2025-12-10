import type { Member, Result, ApiError } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberService extends ICrudService<Member> {
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; 
  addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>>; 
  updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>>; 
  getRelatives(memberId: string): Promise<Result<Member[], ApiError>>; 
}
