import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError, Result, ChatResponse, GenerateFamilyDataDto, CombinedAiContentResponse, OcrResultDto } from '@/types'; // Updated imports

export class ApiChatService implements IChatService {
  constructor(private apiClient: ApiClientMethods) {}

    async sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>> {
        const response = await this.apiClient.post<ChatResponse>(`/ai/chat`, { familyId: familyId, sessionId: sessionId, chatInput: message });
      return response;
    }

  async generateFamilyData(command: GenerateFamilyDataDto): Promise<Result<CombinedAiContentResponse, ApiError>> {
    const response = await this.apiClient.post<CombinedAiContentResponse>(
      `/family/${command.familyId}/generate-data`, // Updated endpoint
      command
    );
    return response; // apiClient.post already returns Result<T, ApiError>
  }

  async performOcr(file: File): Promise<Result<OcrResultDto, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    // Assuming a backend endpoint like /ocr/process
    const response = await this.apiClient.post<OcrResultDto>('/ocr/process', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response;
  }
}


