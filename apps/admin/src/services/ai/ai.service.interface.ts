// apps/admin/src/services/ai/ai.service.interface.ts

import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto, GenerateStoryCommand, GenerateStoryResponseDto } from '@/types/ai';
import type { BiographyStyle, BiographyResultDto } from '@/types/biography';
import type { AnalyzedDataDto } from '@/types/ai'; // NEW IMPORT

export interface IAiService {
  analyzePhoto(command: { Input: AiPhotoAnalysisInputDto }): Promise<Result<PhotoAnalysisResultDto, ApiError>>; // UPDATED
  generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto, ApiError>>;
  analyzeContent(content: string, sessionId: string, familyId: string): Promise<Result<AnalyzedDataDto, ApiError>>; // UPDATED METHOD SIGNATURE
  generateStory(command: GenerateStoryCommand): Promise<Result<GenerateStoryResponseDto, ApiError>>;
}
