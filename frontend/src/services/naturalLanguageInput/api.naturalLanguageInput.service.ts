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
      const apiResponse = await this.http.post<string>(`${this.apiUrl}/generate-data`, requestBody);
      
      if (!apiResponse.ok) {
        return err(apiResponse.error);
      }

      const jsonData = apiResponse.value;
      let dataType = 'Unknown';

      try {
        const parsedJson = JSON.parse(jsonData);
        if (parsedJson.name && parsedJson.visibility) {
          dataType = 'Family';
        } else if (parsedJson.fullName && parsedJson.gender) {
          dataType = 'Member';
        }
      } catch (parseError) {
        console.error('Error parsing generated JSON:', parseError);
        // If parsing fails, dataType remains 'Unknown'
      }

      return ok({ jsonData, dataType });
    } catch (error: any) {
      console.error('Error generating data:', error);
      return err(error.response?.data || error.message);
    }
  }
}