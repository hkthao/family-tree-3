// apps/mobile/family_tree_rn/services/face/api.face.service.ts

import { ApiClientMethods } from '@/types';
import { Result, ApiError, DetectFacesRequest, FaceDetectionResponseDto } from '@/types';
import { IFaceService } from '@/services/face/face.service.interface';

export class ApiFaceService implements IFaceService {
  constructor(private api: ApiClientMethods) {}

  async detectFaces(request: DetectFacesRequest): Promise<Result<FaceDetectionResponseDto>> {
    try {
      const response = await this.api.post<FaceDetectionResponseDto>(`/face/detect`, request);
      return { isSuccess: true, value: response };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }
}
