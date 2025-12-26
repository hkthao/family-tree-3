import type { IEventService } from './event.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { EventDto, AddEventDto, UpdateEventDto } from '@/types';
import type { Result } from '@/types';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiEventService extends ApiCrudService<EventDto, AddEventDto, UpdateEventDto> implements IEventService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/event'); // Base path for event API
  }

  /**
   * Fetches a list of all events for a specific family.
   * @param familyId The ID of the family.
   * @returns A promise that resolves to a Result object containing an array of Event objects or an ApiError.
   */
  async getEventsByFamilyId(familyId: string): Promise<Result<EventDto[]>> {
    const response = await this.http.get<EventDto[]>(`${this.baseUrl}/family/${familyId}`);
    return response;
  }
}