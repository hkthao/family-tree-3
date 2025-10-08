import type { Result } from '@/types/common/result';
import type { BiographyResultDto, AIProviderDto, AIProviderType, AIBiography } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { BiographyStyle } from '@/types';

export interface IAIBiographyService {
  generateBiography(
    memberId: string,
    style: BiographyStyle,
    useDBData: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto, ApiError>>;
  getLastAIBiography(memberId: string): Promise<Result<AIBiography | undefined, ApiError>>;
  getAIProviders(): Promise<Result<AIProviderDto[], ApiError>>;
  saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
    provider: AIProviderType;
    userPrompt: string;
    generatedFromDB: boolean;
    tokensUsed: number;
  }): Promise<Result<string, ApiError>>; // Returns ID of saved biography
}
