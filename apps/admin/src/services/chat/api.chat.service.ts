import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError, Result, ChatResponse, GenerateAiContentCommand, GenerateAiContentResponse } from '@/types'; // Import types

export class ApiChatService implements IChatService {
  constructor(private apiClient: ApiClientMethods) {}

    async sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>> {
        const response = await this.apiClient.post<ChatResponse>(`/ai/chat`, { familyId: familyId, sessionId: sessionId, chatInput: message });
      return response;
    }

  async generateAiContent(command: GenerateAiContentCommand): Promise<Result<GenerateAiContentResponse, ApiError>> {
    try {
      const response = await this.apiClient.post<GenerateAiContentResponse>(
        `/ai/generate-content`,
        command
      );
      return Result.success(response.data);
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message,
        statusCode: error.response?.status || 500,
      };
      return Result.fail(apiError);
    }
  }
}

