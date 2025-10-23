import type { Event, Family, Member } from '@/types';

export interface GeneratedDataResponse {
  families: Omit<Family, 'id'>[];
  members: Member[];
  events: Event[];
  dataType?: string; // Add dataType property
}

export interface GenerateDataRequest {
  prompt: string;
}