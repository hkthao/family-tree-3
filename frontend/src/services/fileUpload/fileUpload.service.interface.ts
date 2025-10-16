import type { Result } from '@/types';
import type { ApiError } from '@/plugins/axios';

export interface IFileUploadService {
  uploadFile(file: File): Promise<Result<string, ApiError>>;
}
