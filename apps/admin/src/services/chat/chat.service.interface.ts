import type { ApiError, Result } from "@/types";

export interface IChatService {
      sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<string, ApiError>>; // Updated signature
}