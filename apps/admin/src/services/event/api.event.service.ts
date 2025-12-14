import type { IEventService } from './event.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Event } from '@/types';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiEventService extends ApiCrudService<Event> implements IEventService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/event'); // Base path for event API
  }
}