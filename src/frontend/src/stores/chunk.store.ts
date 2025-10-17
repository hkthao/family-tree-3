import { defineStore } from 'pinia';
import type { TextChunk } from '@/types';
import i18n from '@/plugins/i18n';

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
        const result = await this.services.chunk.uploadFile(file, metadata);
        if (result.ok) {
          // Initialize chunks with approved status
          this.chunks = result.value.map((chunk) => ({
            ...chunk,
            approved: true,
          }));
        } else {
          this.error = i18n.global.t('chunkAdmin.uploadError');
          console.error(result.error);
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('chunkAdmin.uploadError');
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
        const result = await this.services.chunk.approveChunks(chunksToApprove);
        if (result.ok) {
          // Optionally, clear approved chunks from the store or update their status
          // For now, we'll just clear all chunks after successful approval
          this.clearChunks();
        } else {
          this.error = i18n.global.t('chunkAdmin.approveError');
          console.error(result.error);
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('chunkAdmin.approveError');
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