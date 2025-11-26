import type { Result } from '@/types';
import type { DetectedFace } from '@/types';

export interface IFaceService {
  detect(
    imageFile: File,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<{ imageId: string; resizedImageUrl?: string | null; detectedFaces: DetectedFace[] }, Error>>;
  saveLabels(faceLabels: DetectedFace[], imageId: string): Promise<Result<void, Error>>;
  deleteFacesByMemberId(memberId: string): Promise<Result<void, Error>>;
  // Thêm các phương thức khác nếu cần
}
