import type { ApiError, Result, ChatResponse, GenerateFamilyDataDto, CombinedAiContentResponse } from "@/types"; // Updated imports

export interface IChatService {
      sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>>; // Updated signature
      generateFamilyData(command: GenerateFamilyDataDto): Promise<Result<CombinedAiContentResponse, ApiError>>; // Updated method name and types
}