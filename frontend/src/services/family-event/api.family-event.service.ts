import type { IFamilyEventService } from './family-event.service.interface';
import type { FamilyEvent } from '@/types/family';
import type { Paginated } from '@/types/common';
import type { EventFilter } from './family-event.service.interface';
import { safeApiCall } from '@/utils/api';
import type { ApiError } from '@/utils/api';
import type { Result } from '@/types/common';
import type { AxiosInstance } from 'axios';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyEventService implements IFamilyEventService {
  constructor(private http: AxiosInstance) {}

  private apiUrl = `${API_BASE_URL}/family-events`;

  async fetch(): Promise<Result<FamilyEvent[], ApiError>> {
    console.log('Fetching family events from API');
    return safeApiCall(axios.get<FamilyEvent[]>(this.apiUrl));
  }

  async getById(id: string): Promise<Result<FamilyEvent | undefined, ApiError>> {
    console.log(`Fetching family event ${id} from API`);
    return safeApiCall(axios.get<FamilyEvent>(`${this.apiUrl}/${id}`));
  }

  async add(newItem: Omit<FamilyEvent, 'id'>): Promise<Result<FamilyEvent, ApiError>> {
    console.log('Adding family event via API');
    return safeApiCall(axios.post<FamilyEvent>(this.apiUrl, newItem));
  }

  async update(updatedItem: FamilyEvent): Promise<Result<FamilyEvent, ApiError>> {
    console.log(`Updating family event ${updatedItem.id} via API`);
    return safeApiCall(axios.put<FamilyEvent>(`${this.apiUrl}/${updatedItem.id}`, updatedItem));
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    console.log(`Deleting family event ${id} via API`);
    return safeApiCall(axios.delete<void>(`${this.apiUrl}/${id}`));
  }

  async searchItems(
    filters: EventFilter,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Result<Paginated<FamilyEvent>, ApiError>> {
    console.log('Searching family events via API');
    const params = new URLSearchParams();
    // Add filters to params if they exist
    if (filters.familyId) params.append('familyId', filters.familyId);
    if (filters.eventType) params.append('eventType', filters.eventType);
    if (filters.startDate) params.append('startDate', filters.startDate.toISOString());
    if (filters.endDate) params.append('endDate', filters.endDate.toISOString());
    if (filters.location) params.append('location', filters.location);


    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return safeApiCall(axios.get<Paginated<FamilyEvent>>(`${this.apiUrl}?${params.toString()}`));
  }
}
