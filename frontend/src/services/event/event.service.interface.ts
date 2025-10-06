import type { EventFilter, Paginated, Result, Event } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError } from '@/plugins/axios';

export interface IEventService extends ICrudService<Event> {
  // Extend ICrudService
  loadItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number,
  ): Promise<Result<Paginated<Event>, ApiError>>;
}
