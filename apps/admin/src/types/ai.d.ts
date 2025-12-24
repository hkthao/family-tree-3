import { Family } from './family.d';
import { Member } from './member.d';
import { EventDto } from './event.d';

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