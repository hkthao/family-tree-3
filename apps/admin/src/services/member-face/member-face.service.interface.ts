import type { MemberFace, MemberFaceFilter, Paginated, Result, FaceDetectionRessult } from '@/types';
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
    familyId: string,
    resizeImageForAnalysis: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>>;

}
