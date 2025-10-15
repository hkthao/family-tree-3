import {
  type Member,
  type Result,
  ok,
  type MemberFilter,
  type Paginated,
} from '@/types';
import type { IMemberService } from './member.service.interface'; // Import MemberFilter
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

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

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export class ApiMemberService implements IMemberService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/members`;

  async fetch(): Promise<Result<Member[], ApiError>> {
    // Renamed from fetchMembers
    const result = await this.http.get<Member[]>(this.apiUrl);
    if (result.ok) {
      return ok(result.value.map((m) => transformMemberDates(m)));
    }
    return result;
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    const result = await this.http.get<Member[]>(
      `${this.apiUrl}?familyId=${familyId}`,
    );
    if (result.ok) {
      return ok(result.value.map((m) => transformMemberDates(m)));
    }
    return result;
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> {
    // Renamed from getMemberById
    const result = await this.http.get<Member>(`${this.apiUrl}/${id}`);
    if (result.ok) {
      return ok(result.value ? transformMemberDates(result.value) : undefined);
    }
    return result;
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
    // Renamed from addMember
    const apiMember = prepareMemberForApi(newItem);
    const result = await this.http.post<Member>(this.apiUrl, apiMember);
    if (result.ok) {
      return ok(transformMemberDates(result.value));
    }
    return result;
  }

  async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
    const apiMembers = newItems.map(prepareMemberForApi);
    return this.http.post<string[]>(`${this.apiUrl}/bulk-create`, { members: apiMembers });
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    // Renamed from updateMember
    const apiMember = prepareMemberForApi(updatedItem);
    const result = await this.http.put<Member>(
      `${this.apiUrl}/${updatedItem.id}`,
      apiMember,
    );
    if (result.ok) {
      return ok(transformMemberDates(result.value));
    }
    return result;
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    // Renamed from deleteMember
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  async loadItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    const params = new URLSearchParams();
    if (filters.gender) params.append('gender', filters.gender);
    if (filters.familyId) params.append('familyId', filters.familyId);
    if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.sortOrder) params.append('sortOrder', filters.sortOrder);

    // Add pagination parameters
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    const result = await this.http.get<Paginated<Member>>(
      `${this.apiUrl}/search?${params.toString()}`,
    );

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
    params.append('ids', ids.join(','));
    const result = await this.http.get<Member[]>(
      `${this.apiUrl}/by-ids?${params.toString()}`,
    );
    if (result.ok) {
      return ok(result.value.map((m) => transformMemberDates(m)));
    }
    return result;
  }
}
