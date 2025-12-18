import { ref, computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { MemoryItem } from '@/types';
import { EmotionalTag } from '@/types';
import { useMemoryMediaForm, type LocalMemoryMedia } from '@/composables/memory-item/memory-media/useMemoryMediaForm';
import { useMemoryItemRules } from '@/validations/memoryItem.validation';


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

  const mediaManagement = useMemoryMediaForm({
    initialMedia: options.initialMemoryItemData?.memoryMedia || [],
    memoryItemId: form.id,
    readOnly: options.readOnly || false,
  });

  // Watch uploadedFiles from mediaManagement and assign to form.uploadedFiles
  watch(mediaManagement.uploadedFiles, (newFiles) => {
    form.uploadedFiles = newFiles;
  });

  const { rules } = useMemoryItemRules();

  watch(() => options.initialMemoryItemData, (newData) => {
    if (newData) {
      Object.assign(form, {
        ...newData,
        personIds: newData.memoryPersons ? newData.memoryPersons.map(p => p.memberId) : [],
        uploadedFiles: [],
      });
      mediaManagement.memoryMedia.value = [...(newData.memoryMedia || [])];
      mediaManagement.deletedMediaIds.value = [];
    } else {
      Object.assign(form, { ...defaultNewMemoryItem });
      mediaManagement.memoryMedia.value = [];
      mediaManagement.deletedMediaIds.value = [];
    }
  });

  const emotionalTagOptions = computed(() => [
    { title: t('memoryItem.emotionalTag.happy'), value: EmotionalTag.Happy },
    { title: t('memoryItem.emotionalTag.sad'), value: EmotionalTag.Sad },
    { title: t('memoryItem.emotionalTag.proud'), value: EmotionalTag.Proud },
    { title: t('memoryItem.emotionalTag.memorial'), value: EmotionalTag.Memorial },
    { title: t('memoryItem.emotionalTag.neutral'), value: EmotionalTag.Neutral },
  ]);

  const validate = async () => {
    if (!formRef.value) {
      return false;
    }
    const { valid } = await formRef.value.validate();
    return valid;
  };

  const getFormData = (): MemoryItem => {
    const existingMedias: LocalMemoryMedia[] = mediaManagement.memoryMedia.value.filter(media => !media.isNew);
    const dataToReturn: MemoryItem = {
      id: form.id,
      familyId: form.familyId,
      title: form.title,
      description: form.description,
      happenedAt: form.happenedAt,
      emotionalTag: form.emotionalTag,
      memoryMedia: existingMedias,
      personIds: form.personIds,
      deletedMediaIds: mediaManagement.deletedMediaIds.value,
      memoryPersons: []
    };
    return dataToReturn;
  };

  return {
    formRef,
    form,
    emotionalTagOptions,
    mediaManagement,
    validate,
    getFormData,
    newlyUploadedFiles: mediaManagement.newlyUploadedFiles,
    // Expose the rules from useMemoryItemRules
    validationRules: rules,
  };
}