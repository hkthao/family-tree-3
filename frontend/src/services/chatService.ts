import axios from 'axios';
import type { ChatResponse, MessageItem, ChatListItem } from '@/types/chat';

interface ChatRequest {
  message: string;
  sessionId?: string;
}

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

class ChatApiService {
  async sendMessage(
    message: string,
    sessionId?: string,
  ): Promise<ChatResponse> {
    try {
      const payload: ChatRequest = { message, sessionId };
      const response = await axios.post<ChatResponse>(
        `${API_BASE_URL}/api/chat`,
        payload,
      );
      return response.data;
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  }
}

export const chatService = new ChatApiService();
