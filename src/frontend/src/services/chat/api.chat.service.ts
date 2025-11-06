import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import { err } from '@/types'; // Import ok and err functions
import type {  Result } from '@/types'; // Import types
import { useAuthService } from '../auth/authService';

export class ApiChatService implements IChatService {
  private readonly API_URL = `/api/chat/assistant`; // AI assistant endpoint

  constructor(private apiClient: ApiClientMethods) {}

  async sendMessage(message: string): Promise<Result<string, ApiError>> {
    try {
      const response = await this.apiClient.post<string>(this.API_URL, { message: message });
      return response; // Directly return the response, which is already a Result
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

  async *sendMessageStream(message: string): AsyncGenerator<string, void, unknown> {
    try {
      const response = await fetch(this.API_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          // Add authorization header if needed
           'Authorization': `Bearer ${await useAuthService().getAccessToken()}`
        },
        body: JSON.stringify({ message: message }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        const apiError: ApiError = {
          name: 'ApiError',
          message: errorData.message || 'Failed to get streaming response',
          statusCode: response.status,
          details: errorData,
        };
        throw apiError; // Throw the ApiError object
      }

      const reader = response.body?.getReader();
      if (!reader) {
        const apiError: ApiError = {
          name: 'ApiError',
          message: 'Failed to get readable stream',
          statusCode: 500,
        };
        throw apiError;
      }

      const decoder = new TextDecoder('utf-8');

      while (true) {
        const { value, done } = await reader.read();
        if (done) {
          break;
        }

        const chunkText = decoder.decode(value);
        yield chunkText;
      }
    } catch (error: any) {
      console.error('Streaming error:', error);
      // Ensure the thrown error is an ApiError or conforms to it
      if (error.name === 'ApiError') {
        throw error;
      } else {
        const apiError: ApiError = {
          name: 'ApiError',
          message: error.message || 'An unknown streaming error occurred.',
          details: error,
        };
        throw apiError;
      }
    }
  }
}
