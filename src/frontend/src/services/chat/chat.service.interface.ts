import type { ApiError } from "@/plugins/axios";
import type { ChatMessage, Result } from "@/types";

export interface IChatService {
  sendMessage(message: string, history: ChatMessage[]): Promise<Result<string, ApiError>>;
}