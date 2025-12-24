import type { Family } from './family.d';
import type { Member } from './member.d';
import type { EventDto } from './event.d';

export interface CardData {
  id: string;
  type: string;
  title: string;
  summary: string;
}

export interface GenerateFamilyDataCommand {
  familyId: string;
  chatInput: string;
}

export interface CombinedAiContentDto {
  families: Family[];
  members: Member[];
  events: EventDto[];
}

export type CombinedAiContentResponse = CombinedAiContentDto;