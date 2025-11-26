// apps/admin/src/services/ai/ai.service.interface.ts

import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto } from '@/types/ai';
import type { BiographyStyle, BiographyResultDto } from '@/types/biography';
import type { AnalyzedDataDto } from '@/types/natural-language.d'; // NEW IMPORT

export interface IAiService {
  analyzePhoto(input: AiPhotoAnalysisInputDto): Promise<Result<PhotoAnalysisResultDto, ApiError>>;
  generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto, ApiError>>;
  analyzeContent(content: string, sessionId: string): Promise<Result<AnalyzedDataDto, ApiError>>; // NEW METHOD
}
