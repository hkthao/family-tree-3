import type { Event, Family, Member } from '@/types';

export interface GeneratedDataResponse {
  dataType: string;
  families: Family[];
  members: Member[];
  events: Event[];
}

export interface GenerateDataRequest {
  prompt: string;
}