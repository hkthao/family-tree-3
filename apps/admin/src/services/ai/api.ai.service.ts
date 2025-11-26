// apps/admin/src/services/ai/api.ai.service.ts

import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import type { Result } from '@/types'; // Still need Result type
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto } from '@/types/ai';
import type { BiographyStyle, BiographyResultDto } from '@/types/biography';
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // NEW IMPORT

const API_BASE_URL = '/api/memories'; // Base URL for memory-related AI endpoints
const AI_BIOGRAPHY_URL = '/ai/biography'; // Base URL for AI biography generation
const NATURAL_LANGUAGE_URL = '/natural-language'; // Base URL for natural language analysis

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}

  async analyzePhoto(input: AiPhotoAnalysisInputDto): Promise<Result<PhotoAnalysisResultDto, ApiError>> {
    return this.apiClient.post<PhotoAnalysisResultDto>(`${API_BASE_URL}/analyze-photo`, input);
  }

  async generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto, ApiError>> {
    const payload = {
      memberId,
      style,
      generatedFromDB,
      userPrompt,
      language,
    };
    return this.apiClient.post<BiographyResultDto>(AI_BIOGRAPHY_URL, payload);
  }

  async analyzeContent(content: string, sessionId: string): Promise<Result<AnalyzedDataDto, ApiError>> { // NEW METHOD
    return this.apiClient.post<AnalyzedDataDto>(`${NATURAL_LANGUAGE_URL}/analyze`, { content, sessionId });
  }
}
