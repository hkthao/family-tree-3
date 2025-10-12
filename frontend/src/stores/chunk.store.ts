import { defineStore } from 'pinia';
import type { TextChunk } from '@/types';
import { chunkService } from '@/services/chunkService';

interface ChunkState {
  chunks: TextChunk[];
  loading: boolean;
  error: string | null;
}

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
        const responseData = await chunkService.uploadFile(file, metadata);

        // Initialize chunks with approved status
        this.chunks = responseData.map((chunk) => ({
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

    async approveChunks(chunksToApprove: TextChunk[]): Promise<void> {
      this.loading = true;
      this.error = null;
      try {
        await chunkService.approveChunks(chunksToApprove);
        // Optionally, clear approved chunks from the store or update their status
        // For now, we'll just clear all chunks after successful approval
        this.clearChunks();
      } catch (err: any) {
        this.error =
          err.response?.data?.error || err.message || 'Failed to approve chunks.';
        console.error('Approve chunks error:', err);
      } finally {
        this.loading = false;
      }
    },
  },

  getters: {
    approvedChunks: (state) => state.chunks.filter((chunk) => chunk.approved),
    rejectedChunks: (state) => state.chunks.filter((chunk) => !chunk.approved),
  },
});
