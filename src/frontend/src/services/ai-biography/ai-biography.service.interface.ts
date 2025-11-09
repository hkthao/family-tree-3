import type { Result, ApiError, AIProviderDto, BiographyStyle } from '@/types';

export interface IAIBiographyService {
  generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<string, ApiError>>;

  getAIProviders(): Promise<Result<AIProviderDto[], ApiError>>;

  saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
  }): Promise<Result<string, ApiError>>; 
}
