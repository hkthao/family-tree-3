import type { ApiError, Result, ChatResponse, GenerateFamilyDataCommand, CombinedAiContentResponse } from "@/types"; // Updated imports

export interface IChatService {
      sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>>; // Updated signature
      generateFamilyData(command: GenerateFamilyDataCommand): Promise<Result<CombinedAiContentResponse, ApiError>>; // Updated method name and types
}