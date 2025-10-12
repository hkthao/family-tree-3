import axios from 'axios';
import type { TextChunk } from '@/types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export const chunkService = {
  async uploadFile(
    file: File,
    metadata: {
      fileId: string;
      familyId: string;
      category: string;
      createdBy: string;
    },
  ): Promise<TextChunk[]> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('fileId', metadata.fileId);
    formData.append('familyId', metadata.familyId);
    formData.append('category', metadata.category);
    formData.append('createdBy', metadata.createdBy);

    const response = await axios.post<TextChunk[]>(
      `${API_BASE_URL}/chunk/upload`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    );
    return response.data;
  },

  async approveChunks(chunksToApprove: TextChunk[]): Promise<void> {
    await axios.post(`${API_BASE_URL}/chunk/approve`, chunksToApprove);
  },
};
