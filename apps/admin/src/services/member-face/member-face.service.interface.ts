import type { MemberFace, Result, FaceDetectionResult, ApiError, AddMemberFaceDto, UpdateMemberFaceDto } from '@/types';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberFaceService extends ICrudService<MemberFace, AddMemberFaceDto, UpdateMemberFaceDto> {
  detect(
    imageFile: File,
    familyId: string,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<FaceDetectionResult, ApiError>>;

  exportMemberFaces(memberId?: string, familyId?: string): Promise<Result<string, ApiError>>;
  importMemberFaces(memberId: string | undefined, familyId: string | undefined, data: any): Promise<Result<null, ApiError>>;
}
