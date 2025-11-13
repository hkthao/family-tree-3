import { defineStore } from 'pinia';
import { createServices } from '@/services/service.factory';

export const useFileUploadStore = defineStore('fileUpload', {
  state: () => ({
    loading: false,
    error: null as string | null,
    uploadedUrl: null as string | null,
  }),

  actions: {
    async uploadFile(file: File): Promise<boolean> {
      this.loading = true;
      this.error = null;
      this.uploadedUrl = null;
      try {
        const fileUploadService = createServices('real').fileUpload;
        const result = await fileUploadService.uploadFile(file);
        if (result.ok) {
          this.uploadedUrl = result.value;
          return true;
        } else {
          this.error = result.error?.message || 'File upload failed.';
          return false;
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred during file upload.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    reset() {
      this.loading = false;
      this.error = null;
      this.uploadedUrl = null;
    },
  },
});
