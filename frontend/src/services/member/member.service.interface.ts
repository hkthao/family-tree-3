import type { Member } from '@/types/member';
import type { ICrudService } from '../common/crud.service.interface'; // Import ICrudService

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
  fetchMembersByFamilyId(familyId: string): Promise<Member[]>; // Keep specific method
  searchMembers(filters: MemberFilter): Promise<Member[]>; // Keep specific method
}
