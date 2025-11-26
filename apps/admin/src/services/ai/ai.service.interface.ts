// apps/admin/src/services/ai/ai.service.interface.ts

import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto } from '@/types/ai'; // Import new AI DTOs

export interface IAiService {
  analyzePhoto(input: AiPhotoAnalysisInputDto): Promise<Result<PhotoAnalysisResultDto, ApiError>>;
  // Add other AI-related methods here if needed
}
