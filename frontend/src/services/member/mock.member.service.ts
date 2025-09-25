import type { IMemberService, MemberFilter } from './member.service.interface'; // Import MemberFilter
import type { Member } from '@/types/family';
import type { Paginated } from '@/types/common';
import { simulateLatency } from '@/utils/mockUtils';
import type { Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';
import { fixedMockFamilies } from '@/data/mock/fixed.family.mock'; // Import fixed mock families
import { Gender } from '@/types/gender';

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

// Mock data generation (outside the class definition)
let mockMembers: Member[] = [];
for (let i = 1; i <= 1200; i++) {
  mockMembers.push({
    id: i.toString(),
    lastName: `Last${i}`,
    firstName: `First${i}`,
    fullName: `First${i} Last${i}`,
    familyId: fixedMockFamilies[i % 10].id, // Assign to the first 10 fixed families
    gender: i % 2 === 0 ? Gender.Female : Gender.Male,
    dateOfBirth: new Date(1980 + (i % 30), (i % 12), (i % 28) + 1),
    dateOfDeath: i % 7 === 0 ? new Date(2010 + (i % 10), (i % 12), (i % 28) + 1) : undefined, // Add some death dates
    birthDeathYears: `(${1980 + (i % 30)} - ${i % 7 === 0 ? (2010 + (i % 10)) : ''})`, // Formatted years
    avatarUrl: `https://randomuser.me/api/portraits/${i % 2 === 0 ? 'men' : 'women'}/${i % 100}.jpg`,
    nickname: `Nick${i}`,
    placeOfBirth: `City${i % 10}`,
    placeOfDeath: `City${(i + 1) % 10}`,
    occupation: `Occupation${i % 5}`,
    biography: `Biography of member ${i}`,
  });
}

export class MockMemberService implements IMemberService {
  private _members: Member[] = mockMembers;

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
      const newId = (this._members.length + 1).toString(); // Simple sequential ID
      const memberToAdd = { ...newItem, id: newId };
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

  async getManyByIds(ids: string[]): Promise<Result<Member[], ApiError>> {
    try {
      const members = await simulateLatency(this._members.filter((m) => ids.includes(m.id)).map(m => transformMemberDates(m)));
      return ok(members);
    } catch (e) {
      return err({ message: 'Failed to get members by IDs from mock service.', details: e as Error });
    }
  }
}
