import type { Result, IUploadedFile } from '@/types';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { IFileUploadService } from './file-upload.service.interface';

export class FileUploadApiService implements IFileUploadService {
  constructor(private http: ApiClientMethods) {}

  public async uploadFile(file: File): Promise<Result<IUploadedFile, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<IUploadedFile>(`/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
