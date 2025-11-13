import type { IEventService } from './event.service.interface';
import type { Event, EventFilter } from '@/types';
import type { Paginated, Result } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

export class ApiEventService implements IEventService {
  constructor(private http: ApiClientMethods) {}

  async fetch(): Promise<Result<Event[], ApiError>> {
    console.log('Fetching events from API');
    return this.http.get<Event[]>(`/event`);
  }

  async getById(id: string): Promise<Result<Event | undefined, ApiError>> {
    console.log(`Fetching event ${id} from API`);
    return this.http.get<Event>(`/event/${id}`);
  }

  async add(newItem: Omit<Event, 'id'>): Promise<Result<Event, ApiError>> {
    console.log('Adding event via API');
    return this.http.post<Event>(`/event`, newItem);
  }

  async addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
    console.log('Adding multiple events via API');
    return this.http.post<string[]>(`/event/bulk-create`, newItems);
  }

  async update(updatedItem: Event): Promise<Result<Event, ApiError>> {
    console.log(`Updating event ${updatedItem.id} via API`);
    return this.http.put<Event>(
      `/event/${updatedItem.id}`,
      updatedItem,
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    console.log(`Deleting event ${id} via API`);
    return this.http.delete<void>(`/event/${id}`);
  }

  async loadItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Event>, ApiError>> {
    console.log('Searching events via API');
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
    if (filters.familyId) params.append('familyId', filters.familyId);
    if (filters.eventType) params.append('eventType', filters.eventType.toString());
    if (filters.startDate)
      params.append('startDate', filters.startDate.toISOString());
    if (filters.endDate)
      params.append('endDate', filters.endDate.toISOString());
    if (filters.location) params.append('location', filters.location);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());
    return this.http.get<Paginated<Event>>(
      `/event/search?${params.toString()}`,
    );
  }

  async getByIds(ids: string[]): Promise<Result<Event[], ApiError>> {
    console.log(`Fetching events by IDs: ${ids.join(', ')} from API`);
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<Event[]>(`/event/by-ids?${params.toString()}`);
  }

  async getUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>> {
    console.log('Fetching upcoming events from API');
    const params = new URLSearchParams();
    if (familyId) params.append('familyId', familyId);
    return this.http.get<Event[]>(`/event/upcoming?${params.toString()}`);
  }
}
