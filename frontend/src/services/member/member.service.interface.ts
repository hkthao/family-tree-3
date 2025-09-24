import type { Member } from '@/types/member';
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService
import type { Result } from '@/types/result';
import type { ApiError } from '@/utils/api';

import type { Paginated } from '@/types/pagination';

export interface MemberFilter {
  fullName?: string;
  dateOfBirth?: Date | null;
  dateOfDeath?: Date | null;
  gender?: 'male' | 'female' | 'other' | undefined;
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  familyId?: string | null;
}

export interface IMemberService extends ICrudService<Member> { // Extend ICrudService
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; // Keep specific method
  searchMembers(filters: MemberFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<Member>, ApiError>>; // Keep specific method
}
