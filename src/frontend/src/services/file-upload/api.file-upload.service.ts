import type { Result } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IFileUploadService } from './file-upload.service.interface';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class FileUploadApiService implements IFileUploadService {
  private apiUrl = `${API_BASE_URL}/upload`;

  constructor(private http: ApiClientMethods) {}

  public async uploadFile(file: File): Promise<Result<string, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<string>(this.apiUrl, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
