import { ok, type Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { IFileUploadService } from './fileUpload.service.interface';
import { simulateLatency } from '@/utils/mockUtils';

export class MockFileUploadService implements IFileUploadService {
  public async uploadFile(file: File): Promise<Result<string, ApiError>> {
    console.log('MockFileUploadService: Uploading file', file.name);
    await simulateLatency(500);
    const mockUrl = `https://mock-storage.com/uploads/${file.name}`;
    return ok(mockUrl);
  }
}
