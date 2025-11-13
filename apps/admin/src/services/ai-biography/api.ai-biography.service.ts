import type { IAIBiographyService } from './ai-biography.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, BiographyStyle } from '@/types';

export class ApiAIBiographyService implements IAIBiographyService {
  constructor(private http: ApiClientMethods) {}

  async generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<string, ApiError>> {
    const payload = {
      memberId,
      style,
      generatedFromDB,
      userPrompt,
      language,
    };
    return this.http.post<string>(`/ai/biography`, payload);
  }

  async saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
  }): Promise<Result<string, ApiError>> {
    return this.http.post<string>(`/ai/biography/save`, command);
  }
}