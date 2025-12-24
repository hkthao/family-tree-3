import type { ApiError, Result, ChatResponse, GenerateAiContentCommand, GenerateAiContentResponse } from "@/types";

export interface IChatService {
      sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>>; // Updated signature
      generateAiContent(command: GenerateAiContentCommand): Promise<Result<GenerateAiContentResponse, ApiError>>;
}