import type { Member } from '@/types/member';
import type { IMemberService, MemberFilter } from './member.service.interface'; // Import MemberFilter
import axios from 'axios';

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiMemberService implements IMemberService {
  private apiUrl = `${API_BASE_URL}/members`;

  async fetchMembers(): Promise<Member[]> {
    const response = await axios.get<Member[]>(this.apiUrl);
    return response.data;
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const response = await axios.get<Member[]>(`${this.apiUrl}?familyId=${familyId}`);
    return response.data;
  }

  async getMemberById(id: string): Promise<Member | undefined> {
    const response = await axios.get<Member>(`${this.apiUrl}/${id}`);
    return response.data;
  }

  async addMember(newMember: Omit<Member, 'id'>): Promise<Member> {
    const response = await axios.post<Member>(this.apiUrl, newMember);
    return response.data;
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    const response = await axios.put<Member>(`${this.apiUrl}/${updatedMember.id}`, updatedMember);
    return response.data;
  }

  async deleteMember(id: string): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }

  async searchMembers(filters: MemberFilter): Promise<Member[]> {
    const params = new URLSearchParams();
    if (filters.fullName) params.append('fullName', filters.fullName);
    if (filters.dateOfBirth) params.append('dateOfBirth', filters.dateOfBirth);
    if (filters.dateOfDeath) params.append('dateOfDeath', filters.dateOfDeath);
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.placeOfBirth) params.append('placeOfBirth', filters.placeOfBirth);
    if (filters.placeOfDeath) params.append('placeOfDeath', filters.placeOfDeath);
    if (filters.occupation) params.append('occupation', filters.occupation);
    if (filters.familyId) params.append('familyId', filters.familyId);

    const response = await axios.get<Member[]>(`${this.apiUrl}?${params.toString()}`);
    return response.data;
  }
}
