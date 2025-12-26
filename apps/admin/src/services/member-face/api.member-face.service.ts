import type { MemberFace, FaceDetectionResult, ApiError, AddMemberFaceDto, UpdateMemberFaceDto } from '@/types';
import type { Result } from '@/types';
import type { IMemberFaceService } from './member-face.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiMemberFaceService extends ApiCrudService<MemberFace, AddMemberFaceDto, UpdateMemberFaceDto>  implements IMemberFaceService {   constructor(protected http: ApiClientMethods) {
    super(http, '/member-faces');
  }

  async detect(
    file: File,
    familyId: string,
    resizeImageForAnalysis?: boolean,
    returnCrop?: boolean,
  ): Promise<Result<FaceDetectionResult, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new URLSearchParams();
    params.append('familyId', familyId);
    if (resizeImageForAnalysis !== undefined) {
      params.append('resizeImageForAnalysis', resizeImageForAnalysis.toString());
    }
    if (returnCrop !== undefined) {
      params.append('returnCrop', returnCrop.toString());
    }

    return await this.http.post<FaceDetectionResult>(`/member-faces/detect?${params.toString()}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
