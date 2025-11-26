// apps/admin/src/services/ai/api.ai.service.ts

import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import type { Result } from '@/types'; // Still need Result type
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto } from '@/types/ai';

const API_BASE_URL = '/api/memories'; // Base URL for memory-related AI endpoints

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}

  async analyzePhoto(input: AiPhotoAnalysisInputDto): Promise<Result<PhotoAnalysisResultDto, ApiError>> {
    return this.apiClient.post<PhotoAnalysisResultDto>(`${API_BASE_URL}/analyze-photo`, input);
  }
}
