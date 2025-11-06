import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import { err } from '@/types'; // Import ok and err functions
import type { ChatMessage, Result } from '@/types'; // Import types

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
export class ApiChatService implements IChatService {
  private apiUrl = `${API_BASE_URL}/ai/chat`;

  constructor(private apiClient: ApiClientMethods) {}

  async sendMessage(message: string, history: ChatMessage[]): Promise<Result<string, ApiError>> {
    try {
      const response = await this.apiClient.post<string>(this.apiUrl, { message: message, history: history });
      return response; // The apiClient already returns a Result
    } catch (error: any) {
      // Create a plain object conforming to ApiError interface
      const apiError: ApiError = {
        name: 'ApiError',
        message: error.message,
        statusCode: error.response?.status || 500,
        details: error.response?.data,
      };
      return err(apiError); // Use err function
    }
  }
}
