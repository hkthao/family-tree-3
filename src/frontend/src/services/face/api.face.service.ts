import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result } from '@/types';
import { type DetectedFace } from '@/types';
import { type IFaceService } from './face.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiFaceService implements IFaceService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/faces`;

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
      faceLabels: faceLabels.map(face => ({
        id: face.id,
        boundingBox: face.boundingBox,
        thumbnail: face.thumbnail,
        memberId: face.memberId,
        memberName: face.memberName,
        familyId: face.familyId,
        familyName: face.familyName,
        birthYear: face.birthYear,
        deathYear: face.deathYear,
      })),
    };
    return this.http.post<void>(`${this.apiUrl}/labels`, payload);
  }
}
