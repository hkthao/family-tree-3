import type { IChatService } from './chat.service.interface';
import type { ApiClientMethods, ApiError } from '@/plugins/axios';
import { err } from '@/types'; // Import ok and err functions
import type {  Result } from '@/types'; // Import types
import { useAuthService } from '../auth/authService';

export class ApiChatService implements IChatService {
  private readonly API_URL = `${import.meta.env.VITE_MCP_SERVER_URL}/api/ai/query`; // MCP server endpoint

  constructor(private apiClient: ApiClientMethods) {}

  async sendMessage(message: string): Promise<Result<string, ApiError>> {
    try {
      const response = await this.apiClient.post<string>(this.API_URL, { prompt: message });
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
        body: JSON.stringify({ prompt: message }),
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
        
        try {
          // The backend sends the whole response as a single JSON array of strings.
          const parsedArray = JSON.parse(chunkText);
          if (Array.isArray(parsedArray)) {
            for (const item of parsedArray) {
              yield String(item);
            }
            // The entire response has been processed, so we can exit.
            return;
          }
        } catch (e) {
          // If parsing fails, it might be a plain text stream or SSE.
          // For simplicity in this specific case, we can try to handle it as a plain chunk.
          // The previous logic for SSE (`data:`) can be added here if needed in the future.
          yield chunkText;
        }
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
