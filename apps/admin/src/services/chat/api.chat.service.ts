import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { ApiError, Result } from '@/types'; // Import types
export class ApiChatService implements IChatService {
  constructor(private apiClient: ApiClientMethods) {}
    async sendMessage(sessionId: string, message: string): Promise<Result<string, ApiError>> {
        const response = await this.apiClient.post<string>(`/ai/chat`, { sessionId: sessionId, message: message });
      return response;
    }
}
