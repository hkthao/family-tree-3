import type { Member } from '@/types/family';
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService
import type { Result } from '@/types/common';
import type { ApiError } from '@/utils/api';

import type { Paginated } from '@/types/common';

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
