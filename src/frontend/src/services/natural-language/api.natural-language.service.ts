import type { Result } from '@/types'; // Re-introduce Result import
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';

export interface INaturalLanguageService {
  analyzeContent(content: string, sessionId: string): Promise<Result<string, ApiError>>; 
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL; 
export class ApiNaturalLanguageService implements INaturalLanguageService {
  constructor(private apiClient: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/natural-language`;
  async analyzeContent(content: string, sessionId: string): Promise<Result<string, ApiError>> {
    return this.apiClient.post<string>(this.apiUrl + '/natural-language/analyze', { content, sessionId });
  }
}
