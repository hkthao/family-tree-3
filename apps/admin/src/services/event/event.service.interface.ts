import type { EventFilter, Event, Paginated, Result } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError } from '@/plugins/axios';

export interface IEventService extends ICrudService<Event> {
  // Extend ICrudService
  loadItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: { key: string; order: string }[],
  ): Promise<Result<Paginated<Event>, ApiError>>;
  getUpcomingEvents(familyId?: string): Promise<Result<Event[], ApiError>>;
  addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>>;
}
