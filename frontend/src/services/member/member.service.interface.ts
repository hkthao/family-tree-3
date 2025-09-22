import type { Member } from '@/types/member';

export interface MemberFilter {
  fullName?: string;
  dateOfBirth?: string | null;
  dateOfDeath?: string | null;
  gender?: 'male' | 'female' | 'other' | undefined;
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  familyId?: string | undefined;
}

export interface IMemberService {
  fetchMembers(): Promise<Member[]>;
  fetchMembersByFamilyId(familyId: string): Promise<Member[]>;
  getMemberById(id: string): Promise<Member | undefined>;
  addMember(newMember: Omit<Member, 'id'>): Promise<Member>;
  updateMember(updatedMember: Member): Promise<Member>;
  deleteMember(id: string): Promise<void>;
  searchMembers(filters: MemberFilter): Promise<Member[]>; // Added searchMembers
}
