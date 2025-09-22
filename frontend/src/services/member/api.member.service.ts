import type { Member } from '@/types/member';
import type { IMemberService, MemberFilter } from './member.service.interface'; // Import MemberFilter
import axios from 'axios';

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

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

// Helper function to transform Date objects to ISO strings for API requests
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

export class ApiMemberService implements IMemberService {
  private apiUrl = `${API_BASE_URL}/members`;

  async fetchMembers(): Promise<Member[]> {
    const response = await axios.get<Member[]>(this.apiUrl);
    return response.data.map(transformMemberDates);
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const response = await axios.get<Member[]>(`${this.apiUrl}?familyId=${familyId}`);
    return response.data.map(transformMemberDates);
  }

  async getMemberById(id: string): Promise<Member | undefined> {
    const response = await axios.get<Member>(`${this.apiUrl}/${id}`);
    return response.data ? transformMemberDates(response.data) : undefined;
  }

  async addMember(newMember: Omit<Member, 'id'>): Promise<Member> {
    const apiMember = prepareMemberForApi(newMember);
    const response = await axios.post<Member>(this.apiUrl, apiMember);
    return transformMemberDates(response.data);
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    const apiMember = prepareMemberForApi(updatedMember);
    const response = await axios.put<Member>(`${this.apiUrl}/${updatedMember.id}`, apiMember);
    return transformMemberDates(response.data);
  }

  async deleteMember(id: string): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }

  async searchMembers(filters: MemberFilter): Promise<Member[]> {
    const params = new URLSearchParams();
    if (filters.fullName) params.append('fullName', filters.fullName);
    // dateOfBirth and dateOfDeath in filters are strings, so no transformation needed here
    if (filters.dateOfBirth) params.append('dateOfBirth', filters.dateOfBirth.toISOString());
    if (filters.dateOfDeath) params.append('dateOfDeath', filters.dateOfDeath.toISOString());
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.placeOfBirth) params.append('placeOfBirth', filters.placeOfBirth);
    if (filters.placeOfDeath) params.append('placeOfDeath', filters.placeOfDeath);
    if (filters.occupation) params.append('occupation', filters.occupation);
    if (filters.familyId) params.append('familyId', filters.familyId);

    const response = await axios.get<Member[]>(`${this.apiUrl}?${params.toString()}`);
    return response.data.map(transformMemberDates);
  }
}
