import { ok, err } from '@/types'; // Import from '@/types' as per convention
import type { Result, ApiError } from '@/types'; // Import type Result and ApiError
import type { ApiClientMethods } from '@/plugins/axios'; // Import type
import { createApiError } from '@/plugins/axios'; // Import function
import type { IAiService } from './ai.service.interface'; // Import type

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}

  async generateFamilyKb(familyId: string): Promise<Result<any, ApiError>> {
    try {
      const apiResponse = await this.apiClient.post<any>(`/api/ai/generate-kb/${familyId}`);
      if (apiResponse.ok) {
        return ok(apiResponse.value); // Use apiResponse.value if apiResponse.ok is true
      } else {
        return err(apiResponse.error); // Return the error if apiResponse.ok is false
      }
    } catch (error: any) {
      return err(createApiError(error)); // Use err helper and createApiError
    }
  }
}
