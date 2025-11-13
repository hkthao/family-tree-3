import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result } from '@/types';
import { type DetectedFace } from '@/types';
import { type IFaceService } from './face.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiFaceService implements IFaceService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/face`;

  async detect(imageFile: File): Promise<Result<{ imageId: string; detectedFaces: DetectedFace[] }, ApiError>> {
    const formData = new FormData();
    formData.append('file', imageFile);

    return this.http.post<{ imageId: string; detectedFaces: DetectedFace[] }>(`${this.apiUrl}/detect`, formData, {
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
    return this.http.post<void>(`${this.apiUrl}/labels`, payload);
  }

  async deleteFacesByMemberId(memberId: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`${this.apiUrl}/member/${memberId}`);
  }
}
