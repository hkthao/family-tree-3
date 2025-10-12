import type { Result } from '@/types/common/result';
import type { ChatResponse } from '@/types/chat';
import type { ApiError } from '@/plugins/axios';

export interface IChatService {
  sendMessage(message: string, sessionId?: string): Promise<Result<ChatResponse, ApiError>>;
}
