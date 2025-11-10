export enum EventType {
  Birth = 0,
  Marriage = 1,
  Death = 2,
  Migration = 3,
  Other = 4
}

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
  color?: string;
  // color property
  validationErrors?: string[];
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
  sortBy?: string; 
  sortOrder?: 'asc' | 'desc'; 
}
