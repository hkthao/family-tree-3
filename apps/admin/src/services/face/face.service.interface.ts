import type { Result } from '@/types/result.d';
import type { DetectedFace } from '@/types';

export interface IFaceService {
  detectFaces(base64Image: string): Promise<Result<DetectedFace[]>>;
}
