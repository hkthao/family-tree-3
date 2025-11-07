import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import { err } from '@/types'; // Import ok and err functions
import type { ChatMessage, Result } from '@/types'; // Import types

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
export class ApiChatService implements IChatService {
  private apiUrl = `${API_BASE_URL}/ai/chat`;

  constructor(private apiClient: ApiClientMethods) {}

    async sendMessage(sessionId: string, message: string): Promise<Result<string, ApiError>> {
        const response = await this.apiClient.post<string>(this.apiUrl, { sessionId: sessionId, message: message });
      return response;
    }
}
