
import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError } from '@/types';
import type { Result } from '@/types'; 
import type { GenerateStoryCommand, GenerateStoryResponseDto } from '@/types/ai';
import type { BiographyStyle, BiographyResultDto } from '@/types/biography';
import type { AnalyzedDataDto } from '@/types/ai'; 

const AI_BASE_URL = '/ai'; 

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}

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
    return this.apiClient.post<BiographyResultDto>(`${AI_BASE_URL}/biography`, payload); 
  }

  async analyzeContent(content: string, sessionId: string, familyId: string): Promise<Result<AnalyzedDataDto, ApiError>> { 
    return this.apiClient.post<AnalyzedDataDto>(`${AI_BASE_URL}/generate-family-data`, { content, sessionId, familyId }); 
  }

  async generateStory(command: GenerateStoryCommand): Promise<Result<GenerateStoryResponseDto, ApiError>> {
    return this.apiClient.post<GenerateStoryResponseDto>(`${AI_BASE_URL}/generate-story`, command);
  }
}
