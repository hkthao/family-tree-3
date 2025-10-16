import type { EventFilter, Event } from '@/types/event/event';
import type { Paginated, Result } from '@/types/common';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError } from '@/plugins/axios';

export interface IEventService extends ICrudService<Event> {
  // Extend ICrudService
  loadItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number,
  ): Promise<Result<Paginated<Event>, ApiError>>;
  getUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>>;
  addMultiple(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>>;
}
