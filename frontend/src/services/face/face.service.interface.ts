import type { Result } from '@/types';
import type { DetectedFace } from '@/types';

export interface IFaceService {
  detect(imageFile: File): Promise<Result<DetectedFace[], Error>>;
  // Thêm các phương thức khác nếu cần
}
