import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { type Result } from '@/types';
import { type DetectedFace } from '@/types';
import { type IFaceService } from './face.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiFaceService implements IFaceService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/face`;

  async detect(imageFile: File): Promise<Result<DetectedFace[], ApiError>> {
    const formData = new FormData();
    formData.append('image', imageFile);

    return this.http.post<DetectedFace[]>(`${this.apiUrl}/detect`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
