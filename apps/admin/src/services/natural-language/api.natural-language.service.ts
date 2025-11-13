import type { Result } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // Import AnalyzedDataDto

export interface INaturalLanguageService {
  analyzeContent(content: string, sessionId: string): Promise<Result<AnalyzedDataDto, ApiError>>;
}

export class ApiNaturalLanguageService implements INaturalLanguageService {
  constructor(private apiClient: ApiClientMethods) {}

  async analyzeContent(content: string, sessionId: string): Promise<Result<AnalyzedDataDto, ApiError>> {
    return this.apiClient.post<AnalyzedDataDto>(`/natural-language/analyze`, { content, sessionId });
  }
}
