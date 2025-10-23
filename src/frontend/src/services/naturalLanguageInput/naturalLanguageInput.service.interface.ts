import type { Event, Family, Member, Result, Relationship } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface INaturalLanguageInputService {
  generateFamilyData(prompt: string): Promise<Result<Family[], ApiError>>;
  generateMemberData(prompt: string): Promise<Result<Member[], ApiError>>;
  generateEventData(prompt: string): Promise<Result<Event[], ApiError>>;
  generateRelationshipData(prompt: string): Promise<Result<Relationship[], ApiError>>; 
}