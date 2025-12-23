import type { ApiError, Result, ChatResponse } from "@/types";

export interface IChatService {
      sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>>; // Updated signature
}