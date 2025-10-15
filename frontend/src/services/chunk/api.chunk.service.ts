import type { IChunkService } from './chunk.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { ok, err, type Result } from '@/types/common';
import type { TextChunk } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiChunkService implements IChunkService {
  constructor(private http: ApiClientMethods) {}

  private apiUrl = `${API_BASE_URL}/Chunk`;

  async uploadFile(
    file: File,
    metadata: {
      fileId: string;
      familyId: string;
      category: string;
      createdBy: string;
    },
  ): Promise<Result<TextChunk[], ApiError>> {
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('fileId', metadata.fileId);
      formData.append('familyId', metadata.familyId);
      formData.append('category', metadata.category);
      formData.append('createdBy', metadata.createdBy);

      const response = await this.http.post<TextChunk[]>(`${this.apiUrl}/upload`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      if (response.ok) {
        return ok(response.value);
      } else {
        return err(response.error);
      }
    } catch (error: any) {
      console.error('Error uploading file:', error);
      return err(error.response?.data || error.message);
    }
  }

  async approveChunks(chunks: TextChunk[]): Promise<Result<void, ApiError>> {
    try {
      const response = await this.http.post<void>(`${this.apiUrl}/approve`, chunks);
      if (response.ok) {
        return ok(response.value);
      } else {
        return err(response.error);
      }
    } catch (error: any) {
      console.error('Error approving chunks:', error);
      return err(error.response?.data || error.message);
    }
  }
}