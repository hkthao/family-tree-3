import type { IMemberStoryService } from './memberStory.service.interface';
import type { CreateMemberStory } from '@/types/memberStory';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { ok, type Result, type Paginated, type MemberStoryDto, type SearchStoriesFilter } from '@/types'; // Import 'ok' from '@/types' and SearchStoriesFilter

export class ApiMemberStoryService implements IMemberStoryService {
  private baseRoute = '/member-stories'; // Base route for the MemberStoriesController // Updated

  constructor(private http: ApiClientMethods) {}

  async getById(id: string): Promise<Result<MemberStoryDto | undefined, ApiError>> {
    const result = await this.http.get<MemberStoryDto>(`${this.baseRoute}/detail/${id}`);
    if (result.ok && result.value) {
      // Assuming MemberStoryDto doesn't need date transformations for now
      return result;
    }
    return result;
  }

  async getByIds(ids: string[]): Promise<Result<MemberStoryDto[], ApiError>> {
    // MemberStoriesController does not have a direct endpoint for getByIds.
    // This is a placeholder implementation.
    // For a real scenario, an endpoint like /member-stories/by-ids?ids=id1,id2 would be needed.
    // For now, we'll fetch each by ID or return an empty array.
    const fetchedMemberStories: MemberStoryDto[] = [];
    for (const id of ids) {
      const result = await this.getById(id);
      if (result.ok && result.value) {
        fetchedMemberStories.push(result.value);
      }
    }
    return ok(fetchedMemberStories);
  }

  async add(newItem: CreateMemberStory): Promise<Result<MemberStoryDto, ApiError>> {
    // Backend CreateMemberStoryCommand expects CreateMemberStoryCommand (which maps from CreateMemberStoryDto)
    const result = await this.http.post<string>(this.baseRoute, newItem); // Backend returns Guid
    if (result.ok) {
      // After successful creation, fetch the new member story
      const newMemberStoryId = result.value;
      const newMemberStory = await this.getById(newMemberStoryId);
      if (newMemberStory.ok && newMemberStory.value) {
        return ok(newMemberStory.value);
      }
      return { ok: false, error: { name: 'MemberStoryCreationError', message: 'Failed to retrieve newly created member story.' } };
    }
    // Cast to match return type, ensuring ApiError is correctly typed
    return { ok: false, error: { name: result.error?.name || 'UnknownError', message: result.error?.message || 'Failed to add member story.' } };
  }

  async update(updatedItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>> { // Changed from UpdateMemberStoryDto
    const result = await this.http.put<void>(`${this.baseRoute}/${updatedItem.id}`, updatedItem);
    if (result.ok) {
        // After successful update, fetch the updated member story
        const updatedMemberStory = await this.getById(updatedItem.id!);
        if (updatedMemberStory.ok && updatedMemberStory.value) {
            return ok(updatedMemberStory.value);
        }
        return { ok: false, error: { name: 'MemberStoryUpdateError', message: 'Failed to retrieve updated member story.' } };
    }
    return { ok: false, error: { name: result.error?.name || 'UnknownError', message: result.error?.message || 'Failed to update member story.' } };
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`${this.baseRoute}/${id}`);
  }

  async loadItems(
    filters: SearchStoriesFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>> {
    return this.searchMemberStories(filters, page, itemsPerPage);
  }

  async searchMemberStories(
    filters: SearchStoriesFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemberStoryDto>, ApiError>> {
    const params = new URLSearchParams();
    params.append('pageNumber', page.toString());
    params.append('pageSize', itemsPerPage.toString());

    if (filters.memberId) {
      params.append('memberId', filters.memberId);
    }
    if (filters.searchQuery) {
      params.append('searchQuery', filters.searchQuery);
    }
    if (filters.sortBy) {
      params.append('sortBy', filters.sortBy);
    }
    if (filters.sortOrder) {
      params.append('sortOrder', filters.sortOrder);
    }

    const result = await this.http.get<Paginated<MemberStoryDto>>(
      `${this.baseRoute}/search?${params.toString()}`,
    );
    return result;
  }
}
