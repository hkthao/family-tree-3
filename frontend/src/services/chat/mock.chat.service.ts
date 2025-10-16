import type { Result } from '@/types/common/result';
import type { ChatResponse } from '@/types/chat';
import type { ApiError } from '@/plugins/axios';
import type { IChatService } from './chat.service.interface';

export class MockChatService implements IChatService {
  public async sendMessage(message: string, sessionId?: string): Promise<Result<ChatResponse, ApiError>> {
    console.log(`MockChatService: Sending message: ${message} with sessionId: ${sessionId}`);
    const mockResponse: ChatResponse = {
      response: `Mock response to: ${message}`,
      sessionId: sessionId || 'mock-session-123',
      model: 'mock-model',
      createdAt: new Date().toISOString(),
      context: [], // Added missing context property
    };
    return Promise.resolve({ ok: true, value: mockResponse });
  }
}
