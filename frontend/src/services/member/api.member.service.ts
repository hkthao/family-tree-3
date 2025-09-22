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

// Helper function to transform API response to Member object (fullName to lastName/firstName)
function transformMemberNames(apiMember: any): Member {
  const member: Member = { ...apiMember };
  if (apiMember.fullName) {
    const parts = apiMember.fullName.split(' ');
    member.lastName = parts.length > 1 ? parts[0] : '';
    member.firstName = parts.length > 1 ? parts.slice(1).join(' ') : parts[0] || '';
  }
  return member;
}

// Helper function to transform Member object to API request format (lastName/firstName to fullName)
function prepareMemberForApi(member: Omit<Member, 'id'> | Member): any {
  const apiMember: any = { ...member };
  apiMember.fullName = `${member.lastName} ${member.firstName}`.trim(); // Construct fullName
  delete apiMember.lastName; // Remove lastName
  delete apiMember.firstName; // Remove firstName

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
    return response.data.map(m => transformMemberDates(transformMemberNames(m)));
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Member[]> {
    const response = await axios.get<Member[]>(`${this.apiUrl}?familyId=${familyId}`);
    return response.data.map(m => transformMemberDates(transformMemberNames(m)));
  }

  async getMemberById(id: string): Promise<Member | undefined> {
    const response = await axios.get<Member>(`${this.apiUrl}/${id}`);
    return response.data ? transformMemberDates(transformMemberNames(response.data)) : undefined;
  }

  async addMember(newMember: Omit<Member, 'id'>): Promise<Member> {
    const apiMember = prepareMemberForApi(newMember);
    const response = await axios.post<Member>(this.apiUrl, apiMember);
    return transformMemberDates(transformMemberNames(response.data));
  }

  async updateMember(updatedMember: Member): Promise<Member> {
    const apiMember = prepareMemberForApi(updatedMember);
    const response = await axios.put<Member>(`${this.apiUrl}/${updatedMember.id}`, apiMember);
    return transformMemberDates(transformMemberNames(response.data));
  }

  async deleteMember(id: string): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }

  async searchMembers(filters: MemberFilter): Promise<Member[]> {
    const params = new URLSearchParams();
    if (filters.fullName) params.append('fullName', filters.fullName);
    // dateOfBirth and dateOfDeath in filters are Date objects, convert to ISO string for API
    if (filters.dateOfBirth) params.append('dateOfBirth', filters.dateOfBirth.toISOString());
    if (filters.dateOfDeath) params.append('dateOfDeath', filters.dateOfDeath.toISOString());
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.placeOfBirth) params.append('placeOfBirth', filters.placeOfBirth);
    if (filters.placeOfDeath) params.append('placeOfDeath', filters.placeOfDeath);
    if (filters.occupation) params.append('occupation', filters.occupation);
    if (filters.familyId) params.append('familyId', filters.familyId);

    const response = await axios.get<Member[]>(`${this.apiUrl}?${params.toString()}`);
    return response.data.map(m => transformMemberDates(transformMemberNames(m)));
  }
}
