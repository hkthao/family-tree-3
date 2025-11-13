import type { Result } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IFileUploadService } from './file-upload.service.interface';

export class FileUploadApiService implements IFileUploadService {
  constructor(private http: ApiClientMethods) {}

  public async uploadFile(file: File): Promise<Result<string, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<string>(`/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
