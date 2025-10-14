import type { GeneratedDataResponse } from '@/types';
import type { Result } from '@/types/common';
import type { ApiError } from '@/plugins/axios';

export interface INaturalLanguageInputService {
  generateData(prompt: string): Promise<Result<GeneratedDataResponse, ApiError>>;
}
