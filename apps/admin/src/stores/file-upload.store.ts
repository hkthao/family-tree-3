import { defineStore } from 'pinia';

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
        const result = await this.services.fileUpload.uploadFile(file);
        if (result.ok) {
          this.uploadedUrl = result.value.display_url;
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
