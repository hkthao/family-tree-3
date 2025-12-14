import type { Gender } from "./member";
import type { ListOptions } from "./pagination";

export enum EventType {
  Birth = 0,
  Marriage = 1,
  Death = 2,
  Migration = 3,
  Other = 4
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
  description?: string;
  startDate: Date | null;
  endDate?: Date | null;
  location?: string;
  familyId: string | null;
  familyName?: string;
  familyAvatarUrl?: string;
  relatedMembers?: RelatedMember[];
  relatedMemberIds?: string[];
  type: EventType;
  color?: string;
  // color property
  validationErrors?: string[];
}

export interface EventFilter extends ListOptions {
  searchQuery?: string;
  type?: EventType;
  eventType?: EventType;
  familyId?: string | null ;
  startDate?: Date | null;
  endDate?: Date | null;
  location?: string;
  memberId?: string | null; // Changed from relatedMemberId to memberId
}