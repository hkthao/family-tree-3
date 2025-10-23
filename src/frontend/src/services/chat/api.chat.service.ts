import type { ChatResponse, Result } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IChatService } from './chat.service.interface';

interface ChatRequest {
  message: string;
  sessionId?: string;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiChatService implements IChatService {
  private apiUrl = `${API_BASE_URL}/chat`;

  constructor(private http: ApiClientMethods) {}

  public async sendMessage(
    message: string,
    sessionId?: string,
  ): Promise<Result<ChatResponse, ApiError>> {
    const payload: ChatRequest = { message, sessionId };
    return this.http.post<ChatResponse>(this.apiUrl, payload);
  }
}

