import type { Member } from '@/types/family';
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService
import type { Result } from '@/types/common';
import type { ApiError } from '@/utils/api';

import type { Paginated } from '@/types/common';
import type { Gender } from '@/types/gender';

export interface MemberFilter {
  fullName?: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender?: Gender | undefined;
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  familyId?: string | null;
  searchQuery?: string; // New property for search term
}

export interface IMemberService extends ICrudService<Member> { // Extend ICrudService
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; // Keep specific method
  searchMembers(filters: MemberFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<Member>, ApiError>>; // Keep specific method
  getManyByIds(ids: string[]): Promise<Result<Member[], ApiError>>; // New method to fetch multiple members by IDs
}
