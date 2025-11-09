import type { IAIBiographyService } from './ai-biography.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, AIProviderDto, BiographyStyle } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiAIBiographyService implements IAIBiographyService {
  private apiUrl = `${API_BASE_URL}/ai`;

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
    return this.http.post<string>(`${this.apiUrl}/biography`, payload);
  }

  async getAIProviders(): Promise<Result<AIProviderDto[], ApiError>> {
    return this.http.get<AIProviderDto[]>(`${this.apiUrl}/biography/providers`);
  }

  async saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
  }): Promise<Result<string, ApiError>> {
    return this.http.post<string>(`${this.apiUrl}/biography/save`, command);
  }
}