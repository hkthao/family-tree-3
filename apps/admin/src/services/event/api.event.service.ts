import type { IEventService } from './event.service.interface';
import type { Event } from '@/types'; 
import type { Result } from '@/types'; 
import type { ApiClientMethods } from '@/plugins/axios'; 
import { ApiCrudService } from '../common/api.crud.service'; 

export class ApiEventService extends ApiCrudService<Event> implements IEventService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/event'); 
  }

  async addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[]>> { 
    return this.http.post<string[]>(`/event/bulk-create`, newItems);
  }

  async getUpcomingEvents(familyId?: string): Promise<Result<Event[]>> { 
    const params = new URLSearchParams();
    if (familyId) params.append('familyId', familyId);
    return this.http.get<Event[]>(`/event/upcoming?${params.toString()}`);
  }
}
