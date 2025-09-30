import type { Event } from '@/types/event/event';
import type { Paginated } from '@/types/common';
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types/common';
import type { ApiError } from '@/utils/api';
import type { EventFilter } from '@/types'; // Import EventType enum

export interface IEventService extends ICrudService<Event> { // Extend ICrudService
  loadItems(
    filters: EventFilter,
    page?: number,
    itemsPerPage?: number
  ): Promise<Result<Paginated<Event>, ApiError>>
}
