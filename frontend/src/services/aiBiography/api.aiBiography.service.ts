import type { IAIBiographyService } from './aiBiography.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { Result, BiographyResultDto, AIProviderDto, BiographyStyle, AIProviderType, AIBiography } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiAIBiographyService implements IAIBiographyService {
  private apiUrl = `${API_BASE_URL}/ai`;

  constructor(private http: ApiClientMethods) {}

  async generateBiography(
    memberId: string,
    style: BiographyStyle,
    useDBData: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto, ApiError>> {
    const payload = {
      memberId,
      style,
      useDBData,
      userPrompt,
      language,
    };
    return this.http.post<BiographyResultDto>(`${this.apiUrl}/biography`, payload);
  }

  async getLastAIBiography(memberId: string): Promise<Result<AIBiography | undefined, ApiError>> {
    return this.http.get<AIBiography | undefined>(`${this.apiUrl}/biography/last/${memberId}`);
  }

  async getAIProviders(): Promise<Result<AIProviderDto[], ApiError>> {
    return this.http.get<AIProviderDto[]>(`${this.apiUrl}/biography/providers`);
  }

  async saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
    provider: AIProviderType;
    userPrompt: string;
    generatedFromDB: boolean;
    tokensUsed: number;
  }): Promise<Result<string, ApiError>> {
    return this.http.post<string>(`${this.apiUrl}/biography/save`, command);
  }
}
