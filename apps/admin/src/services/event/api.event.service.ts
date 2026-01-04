import type { IEventService } from './event.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { EventDto, AddEventDto, UpdateEventDto } from '@/types';
import type { Result, ApiError } from '@/types';
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

  /**
   * Fetches a list of all events for a specific member.
   * @param memberId The ID of the member.
   * @returns A promise that resolves to a Result object containing an array of Event objects or an ApiError.
   */
  async getEventsByMemberId(memberId: string): Promise<Result<EventDto[], ApiError>> {
    const response = await this.http.get<EventDto[]>(`${this.baseUrl}/by-member/${memberId}`);
    return response;
  }

  async exportEvents(familyId?: string): Promise<Result<string, ApiError>> {
    const url = `${this.baseUrl}/export`;
    return this.http.get<string>(url, { params: { familyId } });
  }

  async importEvents(familyId: string, payload: any): Promise<Result<void, ApiError>> {
    const url = `${this.baseUrl}/import`;
        return this.http.post<void>(url, { familyId: familyId, events: payload });
  }
}