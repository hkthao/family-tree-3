<template>
  <v-form :disabled="props.readOnly" ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" v-if="form.medias && form.medias.length > 0">
        <v-carousel cycle hide-delimiter-background :continuous="false" hide-delimiters>
          <v-carousel-item v-for="(media, index) in form.medias" :key="media.id || index">
            <div>
              <v-img :src="media.url" cover class="carousel-image"></v-img>
              <v-btn v-if="!props.readOnly" class="carousel-delete-btn" icon="mdi-delete" color="error" size="small"
                @click="removeMedia(media)"></v-btn>
            </div>
          </v-carousel-item>
        </v-carousel>
      </v-col>
      <v-col v-if="!props.readOnly" cols="12">
        <VFileUpload :label="t('memoryItem.form.mediaFile')" v-model="uploadedFiles" :accept="acceptedMimeTypes"
          data-testid="memory-item-file-upload" multiple
          :rules="[(v: File[]) => (v || []).length <= 5 || t('memoryItem.validations.maxFiles', { max: 5 })]"
          :disabled="props.readOnly"></VFileUpload>
      </v-col>

      <v-col cols="12">
        <v-text-field v-model="form.title" :label="t('memoryItem.form.title')"
          :rules="[v => !!v || t('common.validations.required')]" required data-testid="memory-item-title"
          :readonly="props.readOnly"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="form.description" :label="t('memoryItem.form.description')" rows="3"
          data-testid="memory-item-description" :readonly="props.readOnly"></v-textarea>
      </v-col>
      <v-col cols="12" md="6">
        <VDateInput v-model="form.happenedAt" :label="t('memoryItem.form.happenedAt')"
          data-testid="memory-item-happened-at" :readonly="props.readOnly" clearable append-inner-icon="mdi-calendar">
        </VDateInput>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.emotionalTag" :items="emotionalTagOptions" :label="t('memoryItem.form.emotionalTag')"
          :rules="[v => v !== null && v !== undefined || t('common.validations.required')]" required
          data-testid="memory-item-emotional-tag" item-title="title" item-value="value"
          :readonly="props.readOnly"></v-select>
      </v-col>
      <v-col cols="12">
        <MemberAutocomplete :disabled="props.readOnly" v-model="form.personIds" :family-id="props.familyId"
          :label="t('memoryItem.form.persons')" multiple :read-only="props.readOnly" data-testid="memory-item-persons">
        </MemberAutocomplete>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch, reactive, nextTick, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { MemoryItem as BaseMemoryItem, MemoryMedia as BaseMemoryMedia, MemoryPerson } from '@/types';
import { EmotionalTag } from '@/types';
import { VDateInput } from 'vuetify/labs/VDateInput';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';

// Local interface to extend MemoryItem with the local MemoryMedia type
interface LocalMemoryMedia extends BaseMemoryMedia {
  isNew?: boolean;
  file?: File;
}

interface LocalMemoryItem extends Omit<BaseMemoryItem, 'persons' | 'medias'> {
  personIds: string[]; // Use personIds for autocomplete
  deletedMediaIds?: string[];
  medias: LocalMemoryMedia[];
}

interface MemoryItemFormProps {
  initialMemoryItemData?: BaseMemoryItem;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<MemoryItemFormProps>();

const formRef = ref<VForm | null>(null);
const { t } = useI18n();

const uploadedFiles = ref<File[]>([]);
const deletedMediaIds = ref<string[]>([]);

const defaultNewMemoryItem: LocalMemoryItem = {
  id: '',
  familyId: props.familyId,
  title: '',
  description: undefined,
  happenedAt: undefined,
  emotionalTag: EmotionalTag.Neutral,
  personIds: [],
  medias: [],
  deletedMediaIds: [],
};

// Initialize form with local type
const form = reactive<LocalMemoryItem>(
  props.initialMemoryItemData
    ? {
      ...props.initialMemoryItemData,
      personIds: props.initialMemoryItemData.persons ? props.initialMemoryItemData.persons.map(p => p.memberId) : [],
      medias: props.initialMemoryItemData.medias || [], // Use existing medias
      deletedMediaIds: [],
    }
    : { ...defaultNewMemoryItem },
);

watch(uploadedFiles, (newFiles) => {
  if (newFiles.length === 0) return;
  newFiles.forEach(file => {
    form.medias.push({
      id: `new-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`, // Temporary ID for new files
      memoryItemId: form.id,
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

watch(() => props.initialMemoryItemData, (newData) => {
  if (newData) {
    Object.assign(form, {
      ...newData,
      personIds: newData.persons ? newData.persons.map(p => p.memberId) : [],
      medias: newData.medias || [],
      deletedMediaIds: [],
    });
    deletedMediaIds.value = []; // Reset deleted media IDs on data change
  } else {
    Object.assign(form, { ...defaultNewMemoryItem });
    deletedMediaIds.value = []; // Reset deleted media IDs on data change
  }
});

const emotionalTagOptions = computed(() => [
  { title: t('memoryItem.emotionalTag.happy'), value: EmotionalTag.Happy },
  { title: t('memoryItem.emotionalTag.sad'), value: EmotionalTag.Sad },
  { title: t('memoryItem.emotionalTag.proud'), value: EmotionalTag.Proud },
  { title: t('memoryItem.emotionalTag.memorial'), value: EmotionalTag.Memorial },
  { title: t('memoryItem.emotionalTag.neutral'), value: EmotionalTag.Neutral },
]);



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

const validate = async () => {
  if (!formRef.value) return false;
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = (): BaseMemoryItem => {
  const existingMedias: BaseMemoryMedia[] = form.medias.filter(media => !media.isNew);
  const persons: MemoryPerson[] = form.personIds.map(memberId => ({
    memberId: memberId,
    memberName: undefined, // memberName is not available in the form
  }));

  const dataToReturn: BaseMemoryItem = {
    id: form.id,
    familyId: form.familyId,
    title: form.title,
    description: form.description,
    happenedAt: form.happenedAt,
    emotionalTag: form.emotionalTag,
    medias: existingMedias,
    persons: persons,
    deletedMediaIds: deletedMediaIds.value,
  };
  return dataToReturn;
};

const newlyUploadedFiles = computed(() => {
  return form.medias.filter(media => media.isNew && media.file).map(media => media.file as File);
});

const removeMedia = (mediaToDelete: LocalMemoryMedia) => {
  if (!props.readOnly) {
    if (!mediaToDelete.isNew && mediaToDelete.id) {
      deletedMediaIds.value.push(mediaToDelete.id);
    }
    form.medias = form.medias.filter(media => media.id !== mediaToDelete.id);
    // Revoke object URL for temporary files
    if (mediaToDelete.isNew && mediaToDelete.url) {
      URL.revokeObjectURL(mediaToDelete.url);
    }
  }
};

export interface MemoryItemFormExpose {
  validate: () => Promise<boolean>;
  getFormData: () => BaseMemoryItem;
  newlyUploadedFiles: ComputedRef<File[]>;
}

defineExpose<MemoryItemFormExpose>({
  validate,
  getFormData,
  newlyUploadedFiles,
});
</script>

<style scoped>
.carousel-content {
  position: relative;
  width: 100%;
  height: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.carousel-image,
.carousel-video {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: 5px;
}

.carousel-delete-btn {
  position: absolute;
  bottom: 0px;
  right: 0px;
}
</style>
