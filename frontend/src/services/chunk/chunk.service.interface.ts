import type { Result } from '@/types/common';
import type { ApiError } from '@/plugins/axios';
import type { TextChunk } from '@/types';

export interface IChunkService {
  uploadFile(
    file: File,
    metadata: {
      fileId: string;
      familyId: string;
      category: string;
      createdBy: string;
    },
  ): Promise<Result<TextChunk[], ApiError>>;
  approveChunks(chunks: TextChunk[]): Promise<Result<void, ApiError>>;
}