// apps/mobile/family_tree_rn/services/event/event.service.interface.ts

import { Result } from '@/types';
import { EventDto, PaginatedList, SearchPublicEventsQuery, GetPublicUpcomingEventsQuery } from '@/types';

export interface IEventService {
  getEventById(id: string): Promise<Result<EventDto>>;
  searchEvents(query: SearchPublicEventsQuery): Promise<Result<PaginatedList<EventDto>>>;
  getUpcomingEvents(query: GetPublicUpcomingEventsQuery): Promise<Result<EventDto[]>>;
}
