import type { Member } from '@/types/member';
import type { IMemberService } from './member.service.interface';
import { generateMockMembers, generateMockMember } from '@/data/mock/member.mock'; // Import generateMockMember for ID generation

export class MockMemberService implements IMemberService {
  private members: Member[] = generateMockMembers(20); // Start with 20 mock members

  private simulateLatency<T>(data: T): Promise<T> {
    return new Promise((resolve) => setTimeout(() => resolve(data), 300));
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
    // Use generateMockMember to get a proper UUID for the ID
    const memberToAdd: Member = { ...newMember, id: generateMockMember().id };
    this.members.push(memberToAdd);
    return this.simulateLatency(memberToAdd);
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    const index = this.members.findIndex((m) => m.id === updatedMember.id);
    if (index !== -1) {
      this.members[index] = updatedMember;
      return this.simulateLatency(updatedMember);
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
}
