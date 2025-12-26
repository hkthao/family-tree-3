import type { ApiError, Result, ChatResponse, GenerateFamilyDataDto, CombinedAiContentResponse, OcrResultDto, ImageUploadResultDto } from '@/types';

export interface IChatService {
  sendMessage(familyId: string, sessionId: string, message: string): Promise<Result<ChatResponse, ApiError>>; // Updated signature
  generateFamilyData(command: GenerateFamilyDataDto): Promise<Result<CombinedAiContentResponse, ApiError>>;
  performOcr(file: File): Promise<Result<OcrResultDto, ApiError>>;
  uploadFile(file: File): Promise<Result<ImageUploadResultDto, ApiError>>;
}