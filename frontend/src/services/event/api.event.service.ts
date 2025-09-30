import type { IEventService } from './event.service.interface';
import type { Event, Paginated, Result, EventFilter } from '@/types';
import { safeApiCall } from '@/utils/api';
import type { ApiError } from '@/utils/api';
import type { AxiosInstance } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiEventService implements IEventService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/events`;

  async fetch(): Promise<Result<Event[], ApiError>> {
    console.log('Fetching events from API');
    return safeApiCall(this.http.get<Event[]>(this.apiUrl));
  }

  async getById(id: string): Promise<Result<Event | undefined, ApiError>> {
    console.log(`Fetching event ${id} from API`);
    return safeApiCall(this.http.get<Event>(`${this.apiUrl}/${id}`));
  }

  async add(newItem: Omit<Event, 'id'>): Promise<Result<Event, ApiError>> {
    console.log('Adding event via API');
    return safeApiCall(this.http.post<Event>(this.apiUrl, newItem));
  }

  async update(updatedItem: Event): Promise<Result<Event, ApiError>> {
    console.log(`Updating event ${updatedItem.id} via API`);
    return safeApiCall(
      this.http.put<Event>(`${this.apiUrl}/${updatedItem.id}`, updatedItem),
    );
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    console.log(`Deleting event ${id} via API`);
    return safeApiCall(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }

  async loadItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<Paginated<Event>, ApiError>> {
    console.log('Searching events via API');
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.familyId) params.append('familyId', filters.familyId);
    if (filters.eventType) params.append('eventType', filters.eventType);
    if (filters.startDate)
      params.append('startDate', filters.startDate.toISOString());
    if (filters.endDate)
      params.append('endDate', filters.endDate.toISOString());
    if (filters.location) params.append('location', filters.location);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return safeApiCall(
      this.http.get<Paginated<Event>>(`${this.apiUrl}?${params.toString()}`),
    );
  }

  async getByIds(ids: string[]): Promise<Result<Event[], ApiError>> {
    console.log(`Fetching events by IDs: ${ids.join(', ')} from API`);
    const params = new URLSearchParams();
    ids.forEach((id) => params.append('ids', id));
    return safeApiCall(
      this.http.get<Event[]>(`${this.apiUrl}/by-ids?${params.toString()}`),
    );
  }
}
