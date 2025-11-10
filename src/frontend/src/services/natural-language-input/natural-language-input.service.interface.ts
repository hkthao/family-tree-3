import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface INaturalLanguageInputService {
  parseInput(prompt: string): Promise<Result<{ entityType: string; data: any }, ApiError>>;
}