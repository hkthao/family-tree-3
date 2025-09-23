import type { Member } from '@/types/member';
import type { IMemberService, MemberFilter } from './member.service.interface'; // Import MemberFilter
import { generateMockMembers, generateMockMember } from '@/data/mock/member.mock'; // Import generateMockMember for ID generation

export class MockMemberService implements IMemberService {
  private members: Member[] = generateMockMembers(20); // Start with 20 mock members

  private simulateLatency<T>(data: T, error?: string): Promise<T> {
    return new Promise((resolve, reject) => setTimeout(() => {
      if (error) {
        reject(new Error(error));
      } else {
        resolve(data);
      }
    }, 0));
  }

  async fetch(): Promise<Member[]> { // Renamed from fetchMembers
    return this.simulateLatency(this.members);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const filteredMembers = this.members.filter(member => member.familyId === familyId);
    return this.simulateLatency(filteredMembers);
  }

  async getById(id: string): Promise<Member | undefined> { // Renamed from getMemberById
    const member = this.members.find((m) => m.id === id);
    return this.simulateLatency(member);
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Member> { // Renamed from addMember
    const memberToAdd: Member = {
      ...newItem,
      id: generateMockMember().id,
      dateOfBirth: newItem.dateOfBirth ? new Date(newItem.dateOfBirth) : undefined,
      dateOfDeath: newItem.dateOfDeath ? new Date(newItem.dateOfDeath) : undefined,
    };
    this.members.push(memberToAdd);
    return this.simulateLatency(memberToAdd);
  }

  async update(updatedItem: Member): Promise<Member> { // Renamed from updateMember
    const index = this.members.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      const memberToUpdate: Member = {
        ...updatedItem,
        dateOfBirth: updatedItem.dateOfBirth ? new Date(updatedItem.dateOfBirth) : undefined,
        dateOfDeath: updatedItem.dateOfDeath ? new Date(updatedItem.dateOfDeath) : undefined,
      };
      this.members[index] = memberToUpdate;
      return this.simulateLatency(memberToUpdate);
    }
    throw new Error('Member not found');
  }

  async delete(id: string): Promise<void> { // Renamed from deleteMember
    const initialLength = this.members.length;
    this.members = this.members.filter((m) => m.id !== id);
    if (this.members.length === initialLength) {
      throw new Error('Member not found');
    }
    return this.simulateLatency(undefined);
  }

  async searchMembers(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Paginated<Member>> {
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
      filteredMembers = filteredMembers.filter(m => m.placeOfDeath?.toLowerCase().includes(lowerCasePlaceOfBirth));
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

    return this.simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}
