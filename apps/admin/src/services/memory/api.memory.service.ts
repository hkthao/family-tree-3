import type { MemoryDto } from '@/types/memory.d';
import type { IMemoryService, MemoryFilter } from './memory.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { ok, type Result, type Paginated } from '@/types'; // Import 'ok' from '@/types'

export class ApiMemoryService implements IMemoryService {
  private baseRoute = '/memories'; // Base route for the MemoriesController

  constructor(private http: ApiClientMethods) {}

  async fetch(): Promise<Result<MemoryDto[], ApiError>> {
    // This method is part of ICrudService, but MemoriesController doesn't have a direct /memories GET endpoint for all memories.
    // We'll rely on loadItems or getMemoriesByMemberId for listing.
    // For now, returning an empty list or throwing an error, or just not using this method.
    // Let's implement this as fetching all memories (if the backend supports it, which it doesn't directly now)
    // or simply return an empty list or error if not intended.
    // Assuming the main listing will be getMemoriesByMemberId
    return this.http.get<MemoryDto[]>(this.baseRoute); // This endpoint does not exist in backend
  }

  async getById(id: string): Promise<Result<MemoryDto | undefined, ApiError>> {
    const result = await this.http.get<MemoryDto>(`${this.baseRoute}/detail/${id}`);
    if (result.ok && result.value) {
      // Assuming MemoryDto doesn't need date transformations for now
      return result;
    }
    return result;
  }

  async getByIds(ids: string[]): Promise<Result<MemoryDto[], ApiError>> {
    // MemoriesController does not have a direct endpoint for getByIds.
    // This is a placeholder implementation.
    // For a real scenario, an endpoint like /memories/by-ids?ids=id1,id2 would be needed.
    // For now, we'll fetch each by ID or return an empty array.
    const fetchedMemories: MemoryDto[] = [];
    for (const id of ids) {
      const result = await this.getById(id);
      if (result.ok && result.value) {
        fetchedMemories.push(result.value);
      }
    }
    return ok(fetchedMemories);
  }

  async add(newItem: MemoryDto): Promise<Result<MemoryDto, ApiError>> { // Changed from CreateMemoryDto
    // Backend CreateMemoryCommand expects CreateMemoryCommand (which maps from CreateMemoryDto)
    const result = await this.http.post<string>(this.baseRoute, newItem); // Backend returns Guid
    if (result.ok) {
      // After successful creation, fetch the new memory
      const newMemoryId = result.value;
      const newMemory = await this.getById(newMemoryId);
      if (newMemory.ok && newMemory.value) {
        return ok(newMemory.value);
      }
      return { ok: false, error: { name: 'MemoryCreationError', message: 'Failed to retrieve newly created memory.' } };
    }
    // Cast to match return type, ensuring ApiError is correctly typed
    return { ok: false, error: { name: result.error?.name || 'UnknownError', message: result.error?.message || 'Failed to add memory.' } };
  }

  async update(updatedItem: MemoryDto): Promise<Result<MemoryDto, ApiError>> { // Changed from UpdateMemoryDto
    const result = await this.http.put<void>(`${this.baseRoute}/${updatedItem.id}`, updatedItem);
    if (result.ok) {
        // After successful update, fetch the updated memory
        const updatedMemory = await this.getById(updatedItem.id!);
        if (updatedMemory.ok && updatedMemory.value) {
            return ok(updatedMemory.value);
        }
        return { ok: false, error: { name: 'MemoryUpdateError', message: 'Failed to retrieve updated memory.' } };
    }
    return { ok: false, error: { name: result.error?.name || 'UnknownError', message: result.error?.message || 'Failed to update memory.' } };
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`${this.baseRoute}/${id}`);
  }

  async loadItems(
    filters: MemoryFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<MemoryDto>, ApiError>> {
    // MemoriesController only has GetMemoriesByMemberId, not a general loadItems for all memories
    // So this will delegate to getMemoriesByMemberId if memberId is provided, otherwise return empty
    if (filters.memberId) {
      return this.getMemoriesByMemberId(filters.memberId, page, itemsPerPage, filters);
    }

    // If no memberId is provided, and no general memories endpoint, return empty paginated list
    return {
      ok: true,
      value: { items: [], page: 1, totalItems: 0, totalPages: 0 }
    } as Result<Paginated<MemoryDto>, ApiError>;
  }

  async getMemoriesByMemberId(
    memberId: string,
    page: number,
    itemsPerPage: number,
    filters: MemoryFilter, // Add filters parameter
  ): Promise<Result<Paginated<MemoryDto>, ApiError>> {
    const params = new URLSearchParams();
    params.append('pageNumber', page.toString());
    params.append('pageSize', itemsPerPage.toString());

    if (filters.searchQuery) { // Added for search functionality
      params.append('searchQuery', filters.searchQuery);
    }
    if (filters.sortBy) {
      params.append('sortBy', filters.sortBy);
    }
    if (filters.sortOrder) {
      params.append('sortOrder', filters.sortOrder);
    }

    const result = await this.http.get<Paginated<MemoryDto>>(
      `${this.baseRoute}/member/${memberId}?${params.toString()}`,
    );
    return result;
  }

  analyzePhoto(command: FormData): Promise<Result<any, ApiError>> {
    // Not directly part of CRUD, but implement for completeness if needed elsewhere
    return this.http.post<any>(`${this.baseRoute}/analyze-photo`, command);
  }

  generateStory(command: any): Promise<Result<any, ApiError>> {
    // Not directly part of CRUD, but implement for completeness if needed elsewhere
    return this.http.post<any>(`${this.baseRoute}/generate`, command);
  }
}
