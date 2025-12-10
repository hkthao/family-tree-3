import type { MemberFace, MemberFaceFilter, Paginated, Result, FaceDetectionRessult } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberFaceService extends ICrudService<MemberFace> {
  detect(
    imageFile: File,
    familyId: string,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>>;
}
