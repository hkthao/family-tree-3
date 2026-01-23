import type { Gender } from "./member";
import type { ListOptions } from "./pagination";
import type { CalendarType, RepeatRule } from "./enums"; // Import new enums
import type { LunarDate } from "./lunar-date"; // Import LunarDate

export enum EventType {
  Birth = 0,
  Marriage = 1,
  Death = 2,
  Other = 3, // Changed from 4 to 3, removed Migration
}

export interface EventMemberDto {
  memberId: string;
  fullName: string;
  memberName: string;
  avatarUrl?: string;
  gender: Gender;
}

// EventDto as the main representation of an event, returned by services and used for display
export interface EventDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  location?: string;
  locationId?: string | null; // Added
  familyId: string | null;
  familyName?: string;
  familyAvatarUrl?: string;
  eventMembers?: EventMemberDto[];
  eventMemberIds?: string[];
  type: EventType;
  color?: string;
  calendarType: CalendarType;
  solarDate?: Date | null;
  lunarDate?: LunarDate | null;
  repeatRule: RepeatRule;
  currentYearOccurrenceDate?: string; // NEW
  validationErrors?: string[]; // Includes validation errors for display
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

// DTO for adding a new event (without id or validationErrors)
export type AddEventDto = Omit<EventDto, 'id' | 'validationErrors'>;

// DTO for updating an existing event (with id, but without validationErrors as it's input)
export type UpdateEventDto = Omit<EventDto, 'validationErrors'>;

export interface EventFilter extends ListOptions {
  searchQuery?: string;
  type?: EventType;
  eventType?: EventType;
  familyId?: string | null ;
  memberId?: string | null;
  startDate?: Date | null;
  endDate?: Date | null;
  calendarType?: CalendarType | null;
  lunarMonthRange?: number[]; // New property
}

export interface GenerateAndNotifyEventsCommand {
    familyId?: string;
    year?: number;
}