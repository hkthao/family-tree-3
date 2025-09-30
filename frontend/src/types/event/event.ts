import { EventType } from './event-type';

export interface Event {
  id: string;
  name: string;
  description?: string;
  startDate: Date | null;
  endDate?: Date | null;
  location?: string;
  familyId: string | null;
  relatedMembers?: string[];
  type: EventType;
  color?: string; // Added color property
}

export interface EventFilter {
  searchQuery?: string;
  type?: EventType;
  eventType?: EventType;
  familyId?: string | null ;
  startDate?: Date | null;
  endDate?: Date | null;
  location?: string;
  relatedMemberId?: string;
}