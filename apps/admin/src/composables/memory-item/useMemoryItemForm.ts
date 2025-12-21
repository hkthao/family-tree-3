import { ref, computed, reactive, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryItem, MemoryMedia as BaseMemoryMedia } from '@/types';
import { EmotionalTag } from '@/types';
import { useMemoryItemRules } from '@/validations/memoryItem.validation';
import type { VForm } from 'vuetify/components';

const ACCEPTED_MIME_TYPES = [
  'image/jpeg',
  'image/png',
  'image/gif',
  'image/bmp',
  'image/webp',
  'image/svg+xml',
].join(',');

export interface LocalMemoryMedia extends BaseMemoryMedia {
  isNew?: boolean;
  file?: File;
}

interface LocalMemoryItem extends Omit<MemoryItem, 'persons' | 'memoryMedia' | 'deletedMediaIds'> {
  personIds: string[]; // Use personIds for autocomplete
  uploadedFiles?: File[]; // To hold files from VFileUpload
}

interface UseMemoryItemFormOptions {
  initialMemoryItemData?: MemoryItem;
  familyId: string;
  readOnly?: boolean;
}

export function useMemoryItemForm(options: UseMemoryItemFormOptions) {
  const formRef = ref<VForm | null>(null);
  const { t } = useI18n();

  const internalMemoryMedia = ref<LocalMemoryMedia[]>([...(options.initialMemoryItemData?.memoryMedia || [])]);
  const uploadedFiles = ref<File[]>([]);
  const deletedMediaIds = ref<string[]>([]);

  const handleUploadedFilesChange = (newFiles: File[]) => {
    if (newFiles.length === 0) return;
    newFiles.forEach(file => {
      internalMemoryMedia.value.push({
        id: `new-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`, // Temporary ID for new files
        memoryItemId: options.initialMemoryItemData?.id || '', // Use initial ID or empty
        url: URL.createObjectURL(file), // Create a temporary URL for preview
        isNew: true,
        file: file,
      });
    });
    // Clear the file input after processing in the next tick to avoid recursive updates
    nextTick(() => {
      uploadedFiles.value = [];
    });
  };

  watch(uploadedFiles, handleUploadedFilesChange);

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



  const defaultNewMemoryItem: LocalMemoryItem = {
    id: '',
    familyId: options.familyId,
    title: '',
    description: undefined,
    happenedAt: undefined,
    emotionalTag: EmotionalTag.Neutral,
    personIds: [],
    memoryPersons: [],
    uploadedFiles: [],
  };
  const form = reactive<LocalMemoryItem>(
    options.initialMemoryItemData
      ? {
        ...options.initialMemoryItemData,
        personIds: options.initialMemoryItemData.memoryPersons ? options.initialMemoryItemData.memoryPersons.map(p => p.memberId) : [],
        uploadedFiles: [], // Initialize uploaded files as empty for existing items
      }
      : { ...defaultNewMemoryItem },
  );


  const { rules } = useMemoryItemRules();

  const handleInitialMemoryItemDataChange = (newData?: MemoryItem) => {
    if (newData) {
      Object.assign(form, {
        ...newData,
        personIds: newData.memoryPersons ? newData.memoryPersons.map(p => p.memberId) : [],
        uploadedFiles: [],
      });
      internalMemoryMedia.value = [...(newData.memoryMedia || [])]; // Directly assign to internalMedia
      deletedMediaIds.value = []; // Directly assign to deletedMediaIds
    } else {
      Object.assign(form, { ...defaultNewMemoryItem });
      internalMemoryMedia.value = [];
      deletedMediaIds.value = [];
    }
  };

  watch(() => options.initialMemoryItemData, handleInitialMemoryItemDataChange, { deep: true });

  const emotionalTagOptions = computed(() => [
    { title: t('memoryItem.emotionalTag.happy'), value: EmotionalTag.Happy },
    { title: t('memoryItem.emotionalTag.sad'), value: EmotionalTag.Sad },
    { title: t('memoryItem.emotionalTag.proud'), value: EmotionalTag.Proud },
    { title: t('memoryItem.emotionalTag.memorial'), value: EmotionalTag.Memorial },
    { title: t('memoryItem.emotionalTag.neutral'), value: EmotionalTag.Neutral },
  ]);

  const validate = async () => {
    if (!formRef.value) {
      console.error('formRef.value is null in useMemoryItemForm.validate()');
      return false;
    }
    const formInstance = formRef.value as VForm;
    const { valid, errors } = await formInstance.validate();
    if (!valid) {
      console.error('Validation failed for the following fields:', errors);
    }
    return valid;
  };

  const getFormData = (): MemoryItem => {
    const existingMedias: LocalMemoryMedia[] = internalMemoryMedia.value.filter(media => !media.isNew);
    const dataToReturn: MemoryItem = {
      id: form.id,
      familyId: form.familyId,
      title: form.title,
      description: form.description,
      happenedAt: form.happenedAt,
      emotionalTag: form.emotionalTag,
      memoryMedia: existingMedias,
      personIds: form.personIds,
      deletedMediaIds: deletedMediaIds.value, // Direct usage
      memoryPersons: []
    };
    return dataToReturn;
  };

  return {
    state: {
      formRef,
      form,
      emotionalTagOptions,
      memoryMedia: internalMemoryMedia,
      uploadedFiles,
      deletedMediaIds,
      newlyUploadedFiles,
      acceptedMimeTypes: ACCEPTED_MIME_TYPES,
      validationRules: rules,
    },
    actions: {
      validate,
      getFormData,
      removeMedia,
    },
  };
}