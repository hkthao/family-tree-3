import type { MemberFace, Result, FaceDetectionRessult, ApiError } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberFaceService extends ICrudService<MemberFace> {
  detect(
    imageFile: File,
    familyId: string,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>>;
}
