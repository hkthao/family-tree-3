// apps/admin/src/services/ai/api.ai.service.ts

import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import type { Result } from '@/types'; // Still need Result type
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto, GenerateStoryCommand, GenerateStoryResponseDto } from '@/types/ai';
import type { BiographyStyle, BiographyResultDto } from '@/types/biography';
import type { AnalyzedDataDto } from '@/types/ai'; // NEW IMPORT

// const API_BASE_URL = '/api/memories'; // Base URL for memory-related endpoints (non-AI) - REMOVED
const AI_BASE_URL = '/ai'; // Corrected Base URL for AI-related endpoints (without /api)

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}

  async analyzePhoto(command: { Input: AiPhotoAnalysisInputDto }): Promise<Result<PhotoAnalysisResultDto, ApiError>> { // UPDATED
    return this.apiClient.post<PhotoAnalysisResultDto>(`${AI_BASE_URL}/analyze-photo`, command); // UPDATED
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
    return this.apiClient.post<BiographyResultDto>(`${AI_BASE_URL}/biography`, payload); // CORRECTED URL
  }

  async analyzeContent(content: string, sessionId: string, familyId: string): Promise<Result<AnalyzedDataDto, ApiError>> { // UPDATED METHOD SIGNATURE
    return this.apiClient.post<AnalyzedDataDto>(`${AI_BASE_URL}/analyze-natural-language`, { content, sessionId, familyId }); // UPDATED
  }

  async generateStory(command: GenerateStoryCommand): Promise<Result<GenerateStoryResponseDto, ApiError>> {
    return this.apiClient.post<GenerateStoryResponseDto>(`${AI_BASE_URL}/generate-story`, command);
  }
}
