import type { MemberFace, FaceDetectionRessult, ApiError } from '@/types'; 
import type { Result } from '@/types'; 
import type { IMemberFaceService } from './member-face.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiMemberFaceService extends ApiCrudService<MemberFace>  implements IMemberFaceService {
   constructor(protected http: ApiClientMethods) {
    super(http, '/member-faces');
  }

  async detect(
    file: File,
    familyId: string,
    resizeImageForAnalysis?: boolean,
    returnCrop?: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>> {
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

    return await this.http.post<FaceDetectionRessult>(`/member-faces/detect?${params.toString()}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
