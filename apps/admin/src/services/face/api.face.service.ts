import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result } from '@/types';
import { type DetectedFace } from '@/types';
import { type IFaceService } from './face.service.interface';

export class ApiFaceService implements IFaceService {
  constructor(private http: ApiClientMethods) {}

  async detect(imageFile: File): Promise<Result<{ imageId: string; detectedFaces: DetectedFace[] }, ApiError>> {
    const formData = new FormData();
    formData.append('file', imageFile);
    formData.append('fileName', imageFile.name); // Keep fileName
    // formData.append('cloud', 'imgbb'); // Removed cloud
    // formData.append('folder', 'family-tree-face-detection'); // Removed folder

    return this.http.post<{ imageId: string; detectedFaces: DetectedFace[] }>(`/face/detect`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  async saveLabels(faceLabels: DetectedFace[], imageId: string): Promise<Result<void, ApiError>> {
    const payload = {
      imageId: imageId,
      faceLabels: faceLabels,
    };
    return this.http.post<void>(`/face/labels`, payload);
  }

  async deleteFacesByMemberId(memberId: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/face/member/${memberId}`);
  }
}
