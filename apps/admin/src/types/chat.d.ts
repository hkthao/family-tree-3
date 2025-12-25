import type { CombinedAiContentDto } from '@/types/ai.d'; // Import CombinedAiContentDto from ai.d.ts

export interface AiChatMessage {
  sender: 'user' | 'ai';
  text: string;
  intent?: string; // New property
  generatedData?: CombinedAiContentDto; // Also add generatedData for consistency
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
  output?: string;
  generatedData?: CombinedAiContentDto;
  intent?: string;
}
