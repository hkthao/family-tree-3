// apps/admin/src/types/public-event.d.ts
import type { BaseAuditableDto } from './common-dto';
import type { EventType } from './event'; // Reusing EventType from admin event types

export interface EventDto extends BaseAuditableDto {
  id: string;
  familyId: string;
  name?: string;
  description?: string;
  startDate: string;
  endDate?: string;
  location?: string;
  type: EventType;
  relatedMembers: string[];
}

export interface GetEventsQuery { // This is an admin query, but it was in public.d.ts
  familyId?: string;
  startDate?: string;
  endDate?: string;
  type?: EventType;
  relatedMemberId?: string;
}

export interface SearchPublicEventsQuery {
  familyId?: string;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
  type?: EventType;
  relatedMemberId?: string;
  page?: number;
  itemsPerPage?: number;
  sortBy?: string;
  sortOrder?: string; // "asc" or "desc"
}

export interface GetPublicUpcomingEventsQuery {
  familyId?: string;
  startDate?: string;
  endDate?: string;
}
