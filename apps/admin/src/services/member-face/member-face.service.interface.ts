import type { MemberFace, MemberFaceFilter, Paginated, Result, DetectedFace, FaceDetectionRessult } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberFaceService extends ICrudService<MemberFace> {
  loadItems(
    filters: MemberFaceFilter,
    page: number,
    itemsPerPage: number,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<Paginated<MemberFace>, ApiError>>;

  detect(
    imageFile: File,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>>;

}
