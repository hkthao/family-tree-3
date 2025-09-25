import type { Member, MemberFilter } from '@/types/family';
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService
import type { Result } from '@/types/common';
import type { ApiError } from '@/utils/api';

import type { Paginated } from '@/types/common';

export interface IMemberService extends ICrudService<Member> { // Extend ICrudService
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; // Keep specific method
  searchItems(filters: MemberFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<Member>, ApiError>>; // Keep specific method
}
