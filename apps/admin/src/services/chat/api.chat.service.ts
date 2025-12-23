import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError, Result, ChatResponse } from '@/types'; // Import types
export class ApiChatService implements IChatService {
  constructor(private apiClient: ApiClientMethods) {}
    async sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>> {
        const response = await this.apiClient.post<ChatResponse>(`/ai/chat`, { familyId: familyId, sessionId: sessionId, chatInput: message });
      return response;
    }
}
