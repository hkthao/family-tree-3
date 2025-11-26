import type { DetectedFace } from '@/types/face'; // Import DetectedFace
import type { PhotoAnalysisResultDto } from '@/types/ai'; // NEW IMPORT

// ... (other interfaces)

// DTO cho Memory đầy đủ (khi hiển thị chi tiết hoặc trong danh sách)
export interface MemoryDto {
  id?: string; // Made optional to support new creation
  memberId: string | null;
  title: string;
  rawInput?: string; // Changed from 'story' to 'rawInput' and made optional
  story?: string; // New field for AI-generated story
  photoAnalysisId?: string | null;
  photoUrl?: string | null;
  photo?: string | null; // New field for temporary photo data (e.g., base64 string)
  targetFaceId?: string; // New field for the ID of the main character face
  imageSize?: string; // New field for image size, e.g., "512x512"
  exifData?: ExifDataDto; // New field for EXIF metadata
  tags?: string[];
  keywords?: string[];
  eventSuggestion?: string;
  customEventDescription?: string;
  emotionContextTags?: string[];
  customEmotionContext?: string;
  faces?: MemoryFaceDto[]; // Changed back to MemoryFaceDto[]
  perspective?: string; // New field for story perspective
  createdAt?: string; // Made optional to support new creation
  photoAnalysisResult?: PhotoAnalysisResultDto | null;
  // Các trường auditable khác từ BaseAuditableEntity nếu cần hiển thị
}

// DTO for EXIF metadata
export interface ExifDataDto {
  datetime?: string;
  gps?: string; // Or a more structured GPS object if needed
  cameraInfo?: string;
}

// DTO for identified faces in a memory
export interface MemoryFaceDto extends DetectedFace { // Extend DetectedFace
  // faceId, memberId, relationPrompt are already in DetectedFace or should be used from it
  // If you need a specific faceId in MemoryFaceDto different from DetectedFace.id, define it here
  relationPrompt?: string; // Mô tả của người dùng cho "đây là ai?" hoặc "quan hệ?"
}
