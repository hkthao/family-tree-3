import type { Member, Event } from '@/types';

export interface ParsedMember extends Partial<Member> {
  // AI-parsed member data, can be partial
}

export interface ParsedEvent extends Partial<Event> {
  // AI-parsed event data, can be partial
}
