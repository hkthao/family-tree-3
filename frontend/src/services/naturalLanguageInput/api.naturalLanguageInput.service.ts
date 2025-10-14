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

      // Ensure dates are correctly parsed for Member if it's a member
      if (generatedData.dataType === 'Member' && generatedData.member) {
        if (generatedData.member.dateOfBirth) {
          generatedData.member.dateOfBirth = new Date(generatedData.member.dateOfBirth);
        }
        if (generatedData.member.dateOfDeath) {
          generatedData.member.dateOfDeath = new Date(generatedData.member.dateOfDeath);
        }
      }

      return ok(generatedData);
    } catch (error: any) {
      console.error('Error generating data:', error);
      return err(error.response?.data || error.message);
    }
  }
}