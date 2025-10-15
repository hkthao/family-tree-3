import type { GeneratedDataResponse } from '@/types';
import type { Result } from '@/types/common';
import type { ApiError } from '@/plugins/axios';
import type { Family } from '@/types/family';
import type { Member } from '@/types/family/member';
import type { Event } from '@/types/event';

export interface INaturalLanguageInputService {
  generateFamilyData(prompt: string): Promise<Result<Family[], ApiError>>;
  generateMemberData(prompt: string): Promise<Result<Member[], ApiError>>;
  generateEventData(prompt: string): Promise<Result<Event[], ApiError>>;
}