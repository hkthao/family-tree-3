import { ref, computed, reactive, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryItem, MemoryMedia as BaseMemoryMedia } from '@/types';
import type { MediaItem, FamilyMedia } from '@/types/familyMedia'; // Added FamilyMedia for MediaInput compatibility
import { EmotionalTag, MediaType } from '@/types'; // Import MediaType
import { useMemoryItemRules } from '@/validations/memoryItem.validation';
import type { VForm } from 'vuetify/components';

const ACCEPTED_MIME_TYPES = [
  'image/jpeg',
  'image/png',
  'image/gif',
  'image/bmp',
  'image/webp',
  'image/svg+xml',
  'video/mp4',
  'video/webm',
  'audio/mpeg',
  'audio/wav',
  'application/pdf',
].join(',');



// --- Conversion Functions ---
// Converts MemoryMedia to FamilyMedia (for internal use and MediaInput)
const toLocalMemoryMedia = (memoryMediaItem: BaseMemoryMedia, familyId: string): FamilyMedia => {
  return {
    id: memoryMediaItem.id,
    familyId: familyId,
    fileName: memoryMediaItem.url.substring(memoryMediaItem.url.lastIndexOf('/') + 1),
    filePath: memoryMediaItem.url,
    mediaType: memoryMediaItem.type as MediaType || MediaType.Other, // Cast or default
    fileSize: 0, // MemoryMedia does not store fileSize, default to 0
    created: memoryMediaItem.created,
    createdBy: memoryMediaItem.createdBy,
    lastModified: memoryMediaItem.lastModified,
    lastModifiedBy: memoryMediaItem.lastModifiedBy,
  };
};

// Converts FamilyMedia back to MemoryMedia (for saving to backend)
const toMemoryMedia = (familyMediaItem: FamilyMedia): BaseMemoryMedia => {
  return {
    id: familyMediaItem.id,
    memoryItemId: familyMediaItem.familyId || '', // Map familyId back to memoryItemId, ensure it's not null
    url: familyMediaItem.filePath,
    type: familyMediaItem.mediaType?.toString(), // Convert MediaType enum to string
    created: familyMediaItem.created,
    createdBy: familyMediaItem.createdBy,
    lastModified: familyMediaItem.lastModified,
    lastModifiedBy: familyMediaItem.lastModifiedBy,
  };
};
// --- End Conversion Functions ---

export type LocalMemoryMedia = FamilyMedia; // LocalMemoryMedia is now an alias for FamilyMedia

interface LocalMemoryItem extends Omit<MemoryItem, 'persons' | 'memoryMedia' | 'deletedMediaIds'> {
  personIds: string[]; // Use personIds for autocomplete
  location?: string;
  locationId?: string;
}

interface UseMemoryItemFormOptions {
  initialMemoryItemData?: MemoryItem;
  familyId: string;
  readOnly?: boolean;
}

export function useMemoryItemForm(options: UseMemoryItemFormOptions) {
  const formRef = ref<VForm | null>(null);
  const { t } = useI18n();

  const internalMemoryMedia = ref<LocalMemoryMedia[]>([]);
  const deletedMediaIds = ref<string[]>([]);

  onMounted(() => {
    // Convert initial MemoryMedia[] to LocalMemoryMedia[] (which is now FamilyMedia[])
    internalMemoryMedia.value = (options.initialMemoryItemData?.memoryMedia || []).map(mm =>
      toLocalMemoryMedia(mm, options.familyId)
    );
  });

  // Computed property to bridge between MediaInput (FamilyMedia[]) and internal state (LocalMemoryMedia[])
  const mediaInputModel = computed<FamilyMedia[]>({
    get: () => internalMemoryMedia.value,
    set: (newMediaInput: FamilyMedia[]) => {
      // Identify removed media items
      const removedMedia = internalMemoryMedia.value.filter(
        oldItem => !newMediaInput.some(newItem => newItem.id === oldItem.id)
      );

      removedMedia.forEach(media => {
        if (media.id && !deletedMediaIds.value.includes(media.id)) {
          deletedMediaIds.value.push(media.id);
        }
      });

      // If a media item is re-added after being marked for deletion, remove it from deletedMediaIds
      const reAddedMedia = newMediaInput.filter(newItem => deletedMediaIds.value.includes(newItem.id));
      reAddedMedia.forEach(media => {
        deletedMediaIds.value = deletedMediaIds.value.filter(id => id !== media.id);
      });

      internalMemoryMedia.value = newMediaInput as LocalMemoryMedia[]; // Update internal state
    }
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
    location: '',
    locationId: undefined,
  };
  const form = reactive<LocalMemoryItem>(
    options.initialMemoryItemData
      ? {
        ...options.initialMemoryItemData,
        personIds: options.initialMemoryItemData.memoryPersons ? options.initialMemoryItemData.memoryPersons.map(p => p.memberId) : [],
        location: options.initialMemoryItemData.location,
        locationId: options.initialMemoryItemData.locationId,
      }
      : { ...defaultNewMemoryItem },
  );


  const { rules } = useMemoryItemRules();

  const handleInitialMemoryItemDataChange = (newData?: MemoryItem) => {
    if (newData) {
      Object.assign(form, {
        ...newData,
        personIds: newData.memoryPersons ? newData.memoryPersons.map(p => p.memberId) : [],
        locationId: newData.locationId,
      });
      internalMemoryMedia.value = (newData.memoryMedia || []).map(mm =>
        toLocalMemoryMedia(mm, options.familyId)
      );
      deletedMediaIds.value = [];
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
    const dataToReturn: MemoryItem = {
      id: form.id,
      familyId: form.familyId,
      title: form.title,
      description: form.description,
      happenedAt: form.happenedAt,
      emotionalTag: form.emotionalTag,
      location: form.location,
      locationId: form.locationId,
      memoryMedia: internalMemoryMedia.value.map(toMemoryMedia), // Convert back to MemoryMedia
      personIds: form.personIds,
      deletedMediaIds: deletedMediaIds.value,
      memoryPersons: []
    };
    return dataToReturn;
  };

  const addExistingMedia = (mediaItems: MediaItem[]) => {
    mediaItems.forEach(item => {
      internalMemoryMedia.value.push({
        id: item.id,
        familyId: options.familyId,
        fileName: item.url.substring(item.url.lastIndexOf('/') + 1), // Derive fileName from URL
        filePath: item.url,
        mediaType: item.type as MediaType || MediaType.Other, // Map MediaItem.type directly
        fileSize: 0,
        // Add BaseAuditableEntity properties if MediaItem has them, otherwise initialize to null/undefined
        created: undefined,
        createdBy: undefined,
        lastModified: undefined,
        lastModifiedBy: undefined,
      });
    });
  };

  return {
    state: {
      formRef,
      form,
      emotionalTagOptions,
      memoryMedia: mediaInputModel, // Expose the computed property for v-model
      deletedMediaIds,
      acceptedMimeTypes: ACCEPTED_MIME_TYPES,
      validationRules: rules,
    },
    actions: {
      validate,
      getFormData,
      addExistingMedia,
    },
  };
}