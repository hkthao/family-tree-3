// apps/admin/src/types/memory.d.ts

// DTO cho kết quả phân tích ảnh
export interface PhotoAnalysisResultDto {
  id?: string; // ID của kết quả phân tích ảnh
  originalUrl?: string; // URL của ảnh gốc
  description?: string; // Mô tả ảnh
  scene?: string; // Bối cảnh (ví dụ: indoor/outdoor)
  event?: string; // Sự kiện (ví dụ: wedding/family_gathering)
  emotion?: string; // Cảm xúc chính
  faces?: any; // JSON của các đối tượng khuôn mặt
  objects?: string[]; // Danh sách các đối tượng nhận diện được
  yearEstimate?: string; // Ước tính năm chụp
  createdAt?: string; // Thời gian tạo kết quả phân tích
  // Các trường khác nếu có
}

// DTO cho yêu cầu tạo câu chuyện
export interface GenerateStoryRequestDto {
  memberId: string; // ID thành viên
  photoAnalysisId?: string | null; // ID kết quả phân tích ảnh (tùy chọn)
  rawText?: string; // Văn bản thô do người dùng nhập
  style: string; // Phong cách câu chuyện (ví dụ: nostalgic, warm, formal, folk)
  maxWords?: number; // Số từ tối đa
}

// DTO cho một mục trong dòng thời gian
export interface TimelineEntryDto {
  year: number; // Năm
  event: string; // Sự kiện
}

// DTO cho phản hồi tạo câu chuyện
export interface GenerateStoryResponseDto {
  title: string; // Tiêu đề câu chuyện được tạo
  draftStory: string; // Nội dung câu chuyện nháp
  tags?: string[]; // Các thẻ gợi ý
  keywords?: string[]; // Các từ khóa gợi ý
  timeline?: TimelineEntryDto[]; // Dòng thời gian gợi ý
}

// DTO cho Memory đầy đủ (khi hiển thị chi tiết hoặc trong danh sách)
export interface MemoryDto {
  id?: string; // Made optional to support new creation
  memberId: string;
  title: string;
  rawInput?: string; // Changed from 'story' to 'rawInput' and made optional
  story?: string; // New field for AI-generated story
  photoAnalysisId?: string | null;
  photoUrl?: string | null;
  tags?: string[];
  keywords?: string[];
  eventSuggestion?: string;
  customEventDescription?: string;
  emotionContextTags?: string[];
  customEmotionContext?: string;
  faces?: MemoryFaceDto[];
  createdAt?: string; // Made optional to support new creation
  photoAnalysisResult?: PhotoAnalysisResultDto | null;
  // Các trường auditable khác từ BaseAuditableEntity nếu cần hiển thị
}

// DTO for identified faces in a memory
export interface MemoryFaceDto {
  faceId?: string; // ID của khuôn mặt được phát hiện (ví dụ: từ phân tích ảnh)
  memberId: string | null; // ID của thành viên được liên kết với khuôn mặt này
  relationPrompt?: string; // Mô tả của người dùng cho "đây là ai?" hoặc "quan hệ?"
  // Bạn có thể bao gồm URL hoặc ID của khuôn mặt đã cắt nếu cần cho việc hiển thị sau này
}
