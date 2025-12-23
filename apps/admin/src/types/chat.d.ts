export interface AiChatMessage {
  sender: 'user' | 'ai';
  text: string;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface ChatListItem {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  updatedAt: string;
}

export interface MessageItem {
  id: string;
  chatId: string;
  sender: 'user' | 'assistant';
  content: string;
  timestamp: string;
}

export interface ChatResponse {
  output: string;
}
