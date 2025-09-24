import type { IMemberService, MemberFilter } from './member.service.interface'; // Import MemberFilter
import type { Member } from '@/types/member';
import type { Paginated } from '@/types/pagination';
import { fixedMockMembers } from '@/data/mock/fixed.member.mock';
import { simulateLatency } from '@/utils/mockUtils';
import { Result, ok, err } from '@/types/result';
import type { ApiError } from '@/utils/api';

// Helper function to transform date strings to Date objects
function transformMemberDates(member: any): Member {
  if (member.dateOfBirth && typeof member.dateOfBirth === 'string') {
    member.dateOfBirth = new Date(member.dateOfBirth);
  }
  if (member.dateOfDeath && typeof member.dateOfDeath === 'string') {
    member.dateOfDeath = new Date(member.dateOfDeath);
  }
  return member;
}

// Helper function to transform Member object to API request format (lastName/firstName to fullName)
function prepareMemberForApi(member: Omit<Member, 'id'> | Member): any {
  const apiMember: any = { ...member };

  if (apiMember.dateOfBirth instanceof Date) {
    apiMember.dateOfBirth = apiMember.dateOfBirth.toISOString();
  }
  if (apiMember.dateOfDeath instanceof Date) {
    apiMember.dateOfDeath = apiMember.dateOfDeath.toISOString();
  }
  return apiMember;
}

export class MockMemberService implements IMemberService {
  private _members: Member[] = fixedMockMembers;

  get members(): Member[] {
    return [...this._members];
  }

  async fetch(): Promise<Result<Member[], ApiError>> { // Renamed from fetchMembers
    try {
      const members = await simulateLatency(this.members.map(m => transformMemberDates(m)));
      return ok(members);
    } catch (e) {
      return err({ message: 'Failed to fetch members from mock service.', details: e as Error });
    }
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>> {
    try {
      const members = await simulateLatency(this.members.filter(m => m.familyId === familyId).map(m => transformMemberDates(m)));
      return ok(members);
    } catch (e) {
      return err({ message: `Failed to fetch members for family ID ${familyId} from mock service.`, details: e as Error });
    }
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> { // Renamed from getMemberById
    try {
      const member = await simulateLatency(this.members.find((m) => m.id === id));
      return ok(member ? transformMemberDates(member) : undefined);
    } catch (e) {
      return err({ message: `Failed to get member with ID ${id} from mock service.`, details: e as Error });
    }
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> { // Renamed to add
    try {
      const memberToAdd = { ...newItem, id: 'mock-id-' + Math.random().toString(36).substring(7) };
      this._members.push(memberToAdd);
      const addedMember = await simulateLatency(transformMemberDates(memberToAdd));
      return ok(addedMember);
    } catch (e) {
      return err({ message: 'Failed to add member to mock service.', details: e as Error });
    }
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> { // Renamed to update
    try {
      const index = this._members.findIndex((m) => m.id === updatedItem.id);
      if (index !== -1) {
        this._members[index] = updatedItem;
        const updatedMember = await simulateLatency(transformMemberDates(updatedItem));
        return ok(updatedMember);
      }
      return err({ message: 'Member not found', statusCode: 404 });
    } catch (e) {
      return err({ message: 'Failed to update member in mock service.', details: e as Error });
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> { // Renamed to delete
    try {
      const initialLength = this._members.length;
      this._members = this._members.filter((m) => m.id !== id);
      if (this._members.length === initialLength) {
        return err({ message: 'Member not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({ message: 'Failed to delete member from mock service.', details: e as Error });
    }
  }

  async searchMembers(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    try {
      let filteredMembers = this.members;

      if (filters.fullName) {
        const lowerCaseFullName = filters.fullName.toLowerCase();
        filteredMembers = filteredMembers.filter(m =>
          m.lastName.toLowerCase().includes(lowerCaseFullName) ||
          m.firstName.toLowerCase().includes(lowerCaseFullName) ||
          `${m.lastName} ${m.firstName}`.toLowerCase().includes(lowerCaseFullName)
        );
      }
      if (filters.dateOfBirth) {
        // Compare Date objects
        filteredMembers = filteredMembers.filter(m => m.dateOfBirth?.toISOString().split('T')[0] === filters.dateOfBirth?.toISOString().split('T')[0]);
      }
      if (filters.dateOfDeath) {
        // Compare Date objects
        filteredMembers = filteredMembers.filter(m => m.dateOfDeath?.toISOString().split('T')[0] === filters.dateOfDeath?.toISOString().split('T')[0]);
      }
      if (filters.gender) {
        filteredMembers = filteredMembers.filter(m => m.gender === filters.gender);
      }
      if (filters.placeOfBirth) {
        const lowerCasePlaceOfBirth = filters.placeOfBirth.toLowerCase();
        filteredMembers = filteredMembers.filter(m => m.placeOfBirth?.toLowerCase().includes(lowerCasePlaceOfBirth));
      }
      if (filters.placeOfDeath) {
        const lowerCasePlaceOfDeath = filters.placeOfDeath.toLowerCase();
        filteredMembers = filteredMembers.filter(m => m.placeOfDeath?.toLowerCase().includes(lowerCasePlaceOfDeath));
      }
      if (filters.occupation) {
        const lowerCaseOccupation = filters.occupation.toLowerCase();
        filteredMembers = filteredMembers.filter(m => m.occupation?.toLowerCase().includes(lowerCaseOccupation));
      }
      if (filters.familyId) {
        filteredMembers = filteredMembers.filter(m => m.familyId === filters.familyId);
      }

      const totalItems = filteredMembers.length;
      const totalPages = Math.ceil(totalItems / itemsPerPage);
      const start = (page - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      const items = filteredMembers.slice(start, end);

      const paginatedResult = await simulateLatency({
        items,
        totalItems,
        totalPages,
      });
      return ok(paginatedResult);
    } catch (e) {
      return err({ message: 'Failed to search members from mock service.', details: e as Error });
    }
  }
}
