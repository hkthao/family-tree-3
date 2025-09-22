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

  async fetchMembers(): Promise<Member[]> {
    return this.simulateLatency(this.members);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const filteredMembers = this.members.filter(member => member.familyId === familyId);
    return this.simulateLatency(filteredMembers);
  }

  async getMemberById(id: string): Promise<Member | undefined> {
    const member = this.members.find((m) => m.id === id);
    return this.simulateLatency(member);
  }

  async addMember(newMember: Omit<Member, 'id'>): Promise<Member> {
    // Ensure dateOfBirth and dateOfDeath are Date objects if they exist
    const memberToAdd: Member = {
      ...newMember,
      id: generateMockMember().id,
      dateOfBirth: newMember.dateOfBirth ? new Date(newMember.dateOfBirth) : undefined,
      dateOfDeath: newMember.dateOfDeath ? new Date(newMember.dateOfDeath) : undefined,
    };
    this.members.push(memberToAdd);
    return this.simulateLatency(memberToAdd);
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    const index = this.members.findIndex((m) => m.id === updatedMember.id);
    if (index !== -1) {
      // Ensure dateOfBirth and dateOfDeath are Date objects if they exist
      const memberToUpdate: Member = {
        ...updatedMember,
        dateOfBirth: updatedMember.dateOfBirth ? new Date(updatedMember.dateOfBirth) : undefined,
        dateOfDeath: updatedMember.dateOfDeath ? new Date(updatedMember.dateOfDeath) : undefined,
      };
      this.members[index] = memberToUpdate;
      return this.simulateLatency(memberToUpdate);
    }
    throw new Error('Member not found');
  }

  async deleteMember(id: string): Promise<void> {
    const initialLength = this.members.length;
    this.members = this.members.filter((m) => m.id !== id);
    if (this.members.length === initialLength) {
      throw new Error('Member not found');
    }
    return this.simulateLatency(undefined);
  }

  async searchMembers(filters: MemberFilter): Promise<Member[]> {
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

    return this.simulateLatency(filteredMembers);
  }
}
