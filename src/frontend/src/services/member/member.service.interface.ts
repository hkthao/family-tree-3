import type { Member, Result, MemberFilter, Paginated } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberService extends ICrudService<Member> {
  // Extend ICrudService
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; // Keep specific method
  loadItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>>; // Keep specific method
  getByIds(ids: string[]): Promise<Result<Member[], ApiError>>; // New method for fetching multiple members by IDs
  addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>>; // New method for bulk adding members
  updateMemberBiography(memberId: string, biographyContent: string): Promise<Result<void, ApiError>>; // New method for updating member biography
}
