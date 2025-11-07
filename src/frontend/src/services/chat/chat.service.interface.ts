import type { ApiError } from "@/plugins/axios";
import type { Result } from "@/types";

export interface IChatService {
      sendMessage(sessionId: string, message: string): Promise<Result<string, ApiError>>; // Updated signature
}