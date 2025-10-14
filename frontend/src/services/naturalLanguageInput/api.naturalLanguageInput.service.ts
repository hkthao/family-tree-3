import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { GeneratedDataResponse, GenerateDataRequest } from '@/types';
import { ok, err, type Result } from '@/types/common';
import type { INaturalLanguageInputService } from './naturalLanguageInput.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiNaturalLanguageInputService implements INaturalLanguageInputService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/NaturalLanguageInput`;

  async generateData(prompt: string): Promise<Result<GeneratedDataResponse, ApiError>> {
    try {
      const requestBody: GenerateDataRequest = { prompt };
      const apiResponse = await this.http.post<GeneratedDataResponse>(`${this.apiUrl}/generate-data`, requestBody);
      
      if (!apiResponse.ok) {
        return err(apiResponse.error);
      }

      const generatedData = apiResponse.value;

      // Ensure dates are correctly parsed for Members
      generatedData.members.forEach(member => {
        if (member.dateOfBirth) {
          member.dateOfBirth = new Date(member.dateOfBirth);
        }
        if (member.dateOfDeath) {
          member.dateOfDeath = new Date(member.dateOfDeath);
        }
      });

      return ok(generatedData);
    } catch (error: any) {
      console.error('Error generating data:', error);
      return err(error.response?.data || error.message);
    }
  }
}