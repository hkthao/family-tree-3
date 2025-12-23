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

export interface RelatedMember {
  id: string;
  fullName: string;
  avatarUrl?: string;
  gender: Gender;
}

export interface Event {
  id: string;
  name: string;
  code: string;
  description?: string;
  familyId: string | null;
  familyName?: string;
  familyAvatarUrl?: string;
  relatedMembers?: RelatedMember[];
  relatedMemberIds?: string[];
  type: EventType;
  color?: string;
  // New fields
  calendarType: CalendarType;
  solarDate?: Date | null;
  lunarDate?: LunarDate | null;
  repeatRule: RepeatRule;
  // color property
  validationErrors?: string[];
}

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

