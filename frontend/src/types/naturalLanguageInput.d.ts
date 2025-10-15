import type { Family } from '@/types/family';
import type { Member } from '@/types/family/member';
import type { Event } from '@/types/event';

export interface GeneratedDataResponse {
  dataType: string;
  families: Family[];
  members: Member[];
  events: Event[];
}

export interface GenerateDataRequest {
  prompt: string;
}