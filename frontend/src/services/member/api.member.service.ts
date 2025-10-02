import { type Member, type Result, ok, type MemberFilter, type Paginated } from '@/types';
import type { IMemberService } from './member.service.interface'; // Import MemberFilter
import { safeApiCall } from '@/utils/api';
import type { ApiError } from '@/utils/api';
import type { AxiosInstance } from 'axios';

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

export class ApiMemberService implements IMemberService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/members`;

  async fetch(): Promise<Result<Member[], ApiError>> { // Renamed from fetchMembers
    const result = await safeApiCall(this.http.get<Member[]>(this.apiUrl));
    if (result.ok) {
      return ok(result.value.map(m => transformMemberDates(m)));
    }
    return result;
  }

  async fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>> {
    const result = await safeApiCall(this.http.get<Member[]>(`${this.apiUrl}?familyId=${familyId}`));
    if (result.ok) {
      return ok(result.value.map(m => transformMemberDates(m)));
    }
    return result;
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> { // Renamed from getMemberById
    const result = await safeApiCall(this.http.get<Member>(`${this.apiUrl}/${id}`));
    if (result.ok) {
      return ok(result.value ? transformMemberDates(result.value) : undefined);
    }
    return result;
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> { // Renamed from addMember
    const apiMember = prepareMemberForApi(newItem);
    const result = await safeApiCall(this.http.post<Member>(this.apiUrl, apiMember));
    if (result.ok) {
      return ok(transformMemberDates(result.value));
    }
    return result;
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> { // Renamed from updateMember
    const apiMember = prepareMemberForApi(updatedItem);
    const result = await safeApiCall(this.http.put<Member>(`${this.apiUrl}/${updatedItem.id}`, apiMember));
    if (result.ok) {
      return ok(transformMemberDates(result.value));
    }
    return result;
  }

  async delete(id: string): Promise<Result<void, ApiError>> { // Renamed from deleteMember
    return safeApiCall(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }

  async loadItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    const params = new URLSearchParams();
    if (filters.fullName) params.append('fullName', filters.fullName);
    if (filters.dateOfBirth) params.append('dateOfBirth', filters.dateOfBirth.toISOString());
    if (filters.dateOfDeath) params.append('dateOfDeath', filters.dateOfDeath.toISOString());
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.placeOfBirth) params.append('placeOfBirth', filters.placeOfBirth);
    if (filters.placeOfDeath) params.append('placeOfDeath', filters.placeOfDeath);
    if (filters.occupation) params.append('occupation', filters.occupation);
    if (filters.familyId) params.append('familyId', filters.familyId);

    // Add pagination parameters
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    const result = await safeApiCall(this.http.get<Paginated<Member>>(`${this.apiUrl}/search?${params.toString()}`));
    if (result.ok) {
      // Assuming the API returns a Paginated object with items, totalItems, totalPages
      // The items in the response might need date transformation
      result.value.items = result.value.items.map(transformMemberDates);
      return ok(result.value);
    }
    return result;
  }

  async getByIds(ids: string[]): Promise<Result<Member[], ApiError>> {
    const params = new URLSearchParams();
    ids.forEach(id => params.append('ids', id));
    const result = await safeApiCall(this.http.get<Member[]>(`${this.apiUrl}/by-ids?${params.toString()}`));
    if (result.ok) {
      return ok(result.value.map(m => transformMemberDates(m)));
    }
    return result;
  }
}
