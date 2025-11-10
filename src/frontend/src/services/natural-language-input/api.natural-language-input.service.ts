import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result } from '@/types';
import type { INaturalLanguageInputService } from './natural-language-input.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiNaturalLanguageInputService implements INaturalLanguageInputService {
  constructor(private http: ApiClientMethods) {}

  async parseInput(prompt: string): Promise<Result<{ entityType: string; data: any }, ApiError>> {
    return this.http.post<{ entityType: string; data: any }>(`${API_BASE_URL}/ai/parse-input`, { prompt });
  }
}