<template>
  <v-form :disabled="props.readOnly" ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" v-if="memoryMedia && memoryMedia.length > 0">
        <v-carousel :key="memoryMedia.length" cycle hide-delimiter-background :continuous="false" hide-delimiters>
          <v-carousel-item v-for="(media, index) in memoryMedia" :key="media.id || index">
            <div>
              <v-img :src="media.url" cover class="carousel-image"></v-img>
              <v-btn v-if="!props.readOnly" class="carousel-delete-btn" icon="mdi-delete" color="error" size="small"
                @click="removeMedia(media)"></v-btn>
            </div>
          </v-carousel-item>
        </v-carousel>
      </v-col>
      <v-col v-if="!props.readOnly" cols="12">
        <VFileUpload :label="t('memoryItem.form.memoryMediaFile')" v-model="newlyUploadedFiles" :accept="acceptedMimeTypes"
          data-testid="memory-item-file-upload" multiple :rules="validationRules.uploadedFiles"
          :disabled="props.readOnly"></VFileUpload>
      </v-col>

      <v-col cols="12">
        <v-text-field v-model="form.title" :label="t('memoryItem.form.title')" :rules="validationRules.title" required
          data-testid="memory-item-title" :readonly="props.readOnly"></v-text-field>
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
          :rules="validationRules.emotionalTag" required data-testid="memory-item-emotional-tag" item-title="title"
          item-value="value" :readonly="props.readOnly"></v-select>
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
import { type ComputedRef, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MediaItem, MemoryItem } from '@/types';
import { VDateInput } from 'vuetify/labs/VDateInput';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useMemoryItemForm } from '@/composables/memory-item/useMemoryItemForm';
import type { LocalMemoryMedia } from '@/composables/memory-item/useMemoryItemForm'; // Type-only import

interface MemoryItemFormProps {
  initialMemoryItemData?: MemoryItem;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<MemoryItemFormProps>();

const { t } = useI18n();

const {
  state: {
    formRef, // Add formRef here
    form,
    emotionalTagOptions,
    memoryMedia,
    newlyUploadedFiles, // Changed from uploadedFiles
    deletedMediaIds,
    acceptedMimeTypes,
    validationRules,
  },
  actions: { validate, getFormData, removeMedia, addExistingMedia },
} = useMemoryItemForm({
  initialMemoryItemData: props.initialMemoryItemData,
  familyId: props.familyId,
  readOnly: props.readOnly,
});

export interface IMemoryItemFormInstance {
  validate: () => Promise<boolean>;
  getFormData: () => MemoryItem;
  newlyUploadedFiles: Ref<File[]>; // Changed from uploadedFiles
  memoryMedia: Ref<LocalMemoryMedia[]>; // Changed from Ref<LocalMemoryMedia[]>
  deletedMediaIds: Ref<string[]>;
  removeMedia: (mediaToDelete: LocalMemoryMedia) => void;
  acceptedMimeTypes: string;
  addExistingMedia: (mediaItems: any[]) => void; // Changed from MediaItem[] to any[]
}

defineExpose<IMemoryItemFormInstance>({
  validate,
  getFormData,
  newlyUploadedFiles,
  memoryMedia,
  deletedMediaIds,
  removeMedia,
  acceptedMimeTypes,
  addExistingMedia,
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
  object-fit: contain;
  border-radius: 5px;
  height: 500px;
}

.carousel-delete-btn {
  position: absolute;
  top: 5px;
  right: 5px;
}
</style>