interface ChatResponse {
  response: string;
  context: string[];
}

interface MessageItem {
  id: string;
  senderId: string;
  content: string;
  timestamp: string;
  direction: 'outgoing' | 'incoming';
}

interface ChatListItem {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  updatedAt: string;
}

export type { ChatResponse, MessageItem, ChatListItem };
