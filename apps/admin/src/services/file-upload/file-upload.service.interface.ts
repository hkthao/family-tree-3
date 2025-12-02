import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { IUploadedFile } from '@/types';

export interface IFileUploadService {
  uploadFile(file: File): Promise<Result<IUploadedFile, ApiError>>;
}
