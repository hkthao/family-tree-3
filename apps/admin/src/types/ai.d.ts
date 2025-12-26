import type { FamilyDto } from './family.d';
import type { MemberDto } from './member.d';
import type { EventDto } from './event.d';
import type { FamilyLocation } from './familyLocation.d'; // NEW import

export interface CardData {
  id: string;
  type: 'Family' | 'Member' | 'Event';
  title: string;
  summary: string;
  data?: FamilyDto | MemberDto | EventDto;
  isSaved?: boolean; // New property to track if the card has been saved
}

export interface GenerateFamilyDataDto {
  familyId: string;
  chatInput: string;
}

export interface CombinedAiContentDto {
  families: FamilyDto[];
  members: MemberDto[];
  events: EventDto[];
  locations?: FamilyLocation[]; // NEW
}

export type CombinedAiContentResponse = CombinedAiContentDto;