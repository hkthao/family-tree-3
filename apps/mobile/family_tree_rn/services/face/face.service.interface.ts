// apps/mobile/family_tree_rn/services/face/face.service.interface.ts

import { Result } from '@/types';
import { DetectFacesRequest, FaceDetectionResponseDto } from '@/types';

export interface IFaceService {
  detectFaces(request: DetectFacesRequest): Promise<Result<FaceDetectionResponseDto>>;
}
