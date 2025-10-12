import { defineStore } from 'pinia';
import axios from 'axios';
import type { TextChunk } from '@/types';

interface ChunkState {
  chunks: TextChunk[];
  loading: boolean;
  error: string | null;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export const useChunkStore = defineStore('chunk', {
  state: (): ChunkState => ({
    chunks: [],
    loading: false,
    error: null,
  }),

  actions: {
    async uploadFile(
      file: File,
      metadata: {
        fileId: string;
        familyId: string;
        category: string;
        createdBy: string;
      },
    ): Promise<void> {
      this.loading = true;
      this.error = null;
      try {
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

        // Initialize chunks with approved status
        this.chunks = response.data.map((chunk) => ({
          ...chunk,
          approved: true,
        }));
      } catch (err: any) {
        this.error =
          err.response?.data?.error || err.message || 'Failed to upload file.';
        console.error('Upload error:', err);
      } finally {
        this.loading = false;
      }
    },

    setChunkApproval(chunkId: string, approved: boolean): void {
      const chunk = this.chunks.find((c) => c.id === chunkId);
      if (chunk) {
        chunk.approved = approved;
      }
    },

    // Action to clear chunks after processing (e.g., saving to Pinecone)
    clearChunks(): void {
      this.chunks = [];
    },
  },

  getters: {
    approvedChunks: (state) => state.chunks.filter((chunk) => chunk.approved),
    rejectedChunks: (state) => state.chunks.filter((chunk) => !chunk.approved),
  },
});
