import type { ChatResponse, Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IChatService {
  sendMessage(message: string, sessionId?: string): Promise<Result<ChatResponse, ApiError>>;
}
