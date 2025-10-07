import type { Result } from '@/types/common/result';
import type { BiographyResultDto, AIProviderDto } from '@/types';
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
  getLastUserPrompt(memberId: string): Promise<Result<string | undefined, ApiError>>;
  getAIProviders(): Promise<Result<AIProviderDto[], ApiError>>;
}
