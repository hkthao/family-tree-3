import { ref, computed, watch, nextTick } from 'vue';
import type { MemoryMedia as BaseMemoryMedia } from '@/types';
import { useI18n } from 'vue-i18n';

export interface LocalMemoryMedia extends BaseMemoryMedia {
  isNew?: boolean;
  file?: File;
}

interface UseMemoryMediaFormOptions {
  initialMedia: LocalMemoryMedia[];
  memoryItemId: string; // The ID of the memory item
  readOnly: boolean;
}

export function useMemoryMediaForm(options: UseMemoryMediaFormOptions) {
  const { t } = useI18n(); // eslint-disable-line @typescript-eslint/no-unused-vars -- t is used in template if needed

  const internalMemoryMedia = ref<LocalMemoryMedia[]>([...options.initialMedia]); // Manage internal media state
  const uploadedFiles = ref<File[]>([]);
  const deletedMediaIds = ref<string[]>([]);

  watch(uploadedFiles, (newFiles) => {
    if (newFiles.length === 0) return;
    newFiles.forEach(file => {
      internalMemoryMedia.value.push({
        id: `new-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`, // Temporary ID for new files
        memoryItemId: options.memoryItemId,
        url: URL.createObjectURL(file), // Create a temporary URL for preview
        isNew: true,
        file: file,
      });
    });
    // Clear the file input after processing in the next tick to avoid recursive updates
    nextTick(() => {
      uploadedFiles.value = [];
    });
  });

  const removeMedia = (mediaToDelete: LocalMemoryMedia) => {
    if (!options.readOnly) {
      if (!mediaToDelete.isNew && mediaToDelete.id) {
        deletedMediaIds.value.push(mediaToDelete.id);
      }
      internalMemoryMedia.value = internalMemoryMedia.value.filter(media => media.id !== mediaToDelete.id);
      // Revoke object URL for temporary files
      if (mediaToDelete.isNew && mediaToDelete.url) {
        URL.revokeObjectURL(mediaToDelete.url);
      }
    }
  };

  const newlyUploadedFiles = computed(() => {
    return internalMemoryMedia.value.filter(media => media.isNew && media.file).map(media => media.file as File);
  });

  const acceptedMimeTypes = computed(() => {
    return [
      'image/jpeg',
      'image/png',
      'image/gif',
      'image/bmp',
      'image/webp',
      'image/svg+xml',
    ].join(',');
  });

  // Expose what the component needs
  return {
    memoryMedia: internalMemoryMedia, // The managed list of media
    uploadedFiles, // For VFileUpload v-model
    deletedMediaIds, // To be exposed for form submission
    removeMedia,
    newlyUploadedFiles, // To be exposed for form submission
    acceptedMimeTypes, // For VFileUpload accept attribute
  };
}
