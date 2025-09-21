import axios from '../plugins/axios';
import { mockMembers } from '../data/mock/members.mock';
import { simulateAsyncOperation } from '../stores/utils';
import { Family } from '../types/family'; // Assuming Family is needed for member context

export interface Member {
  id: string;
  familyId: string;
  fullName: string;
  givenName?: string;
  nicknames?: string[];
  gender: 'Male' | 'Female' | 'Other';
  dob?: string;
  dod?: string;
  status: 'Alive' | 'Deceased';
  avatarUrl?: string;
  contactEmail?: string;
  contactPhone?: string;
  generation: number;
  orderInFamily?: number;
  description?: string;
  metadata?: Record<string, any>;
  createdAt: string;
  updatedAt: string;
}

export interface MemberServiceType {
  fetchMembers(search?: string, familyId?: string, page?: number, perPage?: number): Promise<{ items: Member[]; total: number }>;
  fetchMemberById(id: string): Promise<Member | undefined>;
  addMember(member: Omit<Member, 'id' | 'createdAt' | 'updatedAt'>): Promise<Member>;
  updateMember(id: string, member: Partial<Omit<Member, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void>;
  removeMember(id: string): Promise<void>;
}

export class RealMemberService implements MemberServiceType {
  async fetchMembers(search?: string, familyId?: string, page?: number, perPage?: number): Promise<{ items: Member[]; total: number }> {
    const response = await axios.get(`/members`, {
      params: { search, familyId, page, perPage },
    });
    return response.data;
  }

  async fetchMemberById(id: string): Promise<Member | undefined> {
    const response = await axios.get(`/members/${id}`);
    return response.data;
  }

  async addMember(member: Omit<Member, 'id' | 'createdAt' | 'updatedAt'>): Promise<Member> {
    const response = await axios.post(`/members`, member);
    return response.data;
  }

  async updateMember(id: string, member: Partial<Omit<Member, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    await axios.put(`/members/${id}`, member);
  }

  async removeMember(id: string): Promise<void> {
    await axios.delete(`/members/${id}`);
  }
}

export class MockMemberService implements MemberServiceType {
  private members: Member[] = [];

  constructor(initialMembers: Member[] = []) {
    this.members = JSON.parse(JSON.stringify(initialMembers)); // Deep copy
  }

  async fetchMembers(search?: string, familyId?: string, page = 1, perPage = 10): Promise<{ items: Member[]; total: number }> {
    let filtered = this.members;
    if (familyId) {
      filtered = filtered.filter(m => m.familyId === familyId);
    }
    if (search) {
      filtered = filtered.filter(m => m.fullName.toLowerCase().includes(search.toLowerCase()));
    }
    const items = filtered.slice((page - 1) * perPage, page * perPage);
    return simulateAsyncOperation({ items, total: filtered.length });
  }

  async fetchMemberById(id: string): Promise<Member | undefined> {
    const member = this.members.find(m => m.id === id);
    return simulateAsyncOperation(member);
  }

  async addMember(member: Omit<Member, 'id' | 'createdAt' | 'updatedAt'>): Promise<Member> {
    const newMember: Member = {
      ...member,
      id: 'mock-' + (this.members.length + 1),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    this.members.push(newMember);
    return simulateAsyncOperation(newMember);
  }

  async updateMember(id: string, member: Partial<Omit<Member, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    const index = this.members.findIndex(m => m.id === id);
    if (index !== -1) {
      this.members[index] = { ...this.members[index], ...member, updatedAt: new Date().toISOString() };
    }
    return simulateAsyncOperation(undefined);
  }

  async removeMember(id: string): Promise<void> {
    const index = this.members.findIndex(m => m.id === id);
    if (index !== -1) {
      this.members.splice(index, 1);
    }
    return simulateAsyncOperation(undefined);
  }
}
