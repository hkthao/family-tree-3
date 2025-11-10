import type { Result, BiographyStyle } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IAIBiographyService {
  generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<string, ApiError>>;

  saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
  }): Promise<Result<string, ApiError>>; 
}
