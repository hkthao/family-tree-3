<template>
  <v-form :disabled="props.readOnly" ref="formRef" @submit.prevent>
    <v-row>
      <v-col v-if="!props.readOnly" cols="12">
        <MediaInput v-model="memoryMedia" :family-id="props.familyId" :initialMediaType="MediaType.Image" data-testid="memory-item-media-input" selectionMode="multiple"
          :disabled="props.readOnly" />
      </v-col>

      <v-col cols="12">
        <v-text-field v-model="form.title" :label="t('memoryItem.form.title')" :rules="validationRules.title" required
          data-testid="memory-item-title" :readonly="props.readOnly"
          prepend-inner-icon="mdi-format-title"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="form.description" :label="t('memoryItem.form.description')" rows="3"
          data-testid="memory-item-description" :readonly="props.readOnly"
          prepend-inner-icon="mdi-text-box-outline"></v-textarea>
      </v-col>
      <v-col cols="12">
        <LocationInputField v-model:model-value="form.location" v-model:location-id="form.locationId" :label="t('memoryItem.form.location')" :family-id="props.familyId"
          :read-only="props.readOnly" prepend-inner-icon="mdi-map-marker"></LocationInputField>
      </v-col>
      <v-col cols="12" md="6">
        <VDateInput v-model="form.happenedAt" :label="t('memoryItem.form.happenedAt')"
          data-testid="memory-item-happened-at" :readonly="props.readOnly" clearable append-inner-icon="mdi-calendar">
        </VDateInput>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.emotionalTag" :items="emotionalTagOptions" :label="t('memoryItem.form.emotionalTag')"
          :rules="validationRules.emotionalTag" required data-testid="memory-item-emotional-tag" item-title="title"
          item-value="value" :readonly="props.readOnly" prepend-inner-icon="mdi-emoticon"></v-select>
      </v-col>
      <v-col cols="12">
        <MemberAutocomplete :disabled="props.readOnly" v-model="form.personIds" :family-id="props.familyId"
          :label="t('memoryItem.form.persons')" multiple :read-only="props.readOnly" data-testid="memory-item-persons"
          prepend-inner-icon="mdi-account-group">
        </MemberAutocomplete>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { MediaType, type MemoryItem } from '@/types';
import { VDateInput } from 'vuetify/labs/VDateInput';
import MediaInput from '@/components/common/MediaInput.vue';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import LocationInputField from '@/components/common/LocationInputField.vue'; // NEW import
import { useMemoryItemForm } from '@/composables/memory-item/useMemoryItemForm';
import type { FamilyMedia } from '@/types/familyMedia'; // Import FamilyMedia

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
    deletedMediaIds,
    acceptedMimeTypes,
    validationRules,

  },
  actions: { validate, getFormData, addExistingMedia },
} = useMemoryItemForm({
  initialMemoryItemData: props.initialMemoryItemData,
  familyId: props.familyId,
  readOnly: props.readOnly,
});

export interface IMemoryItemFormInstance {
  validate: () => Promise<boolean>;
  getFormData: () => MemoryItem;
  memoryMedia: Ref<FamilyMedia[]>;
  deletedMediaIds: Ref<string[]>;
  acceptedMimeTypes: string;
  addExistingMedia: (mediaItems: any[]) => void;
}

defineExpose<IMemoryItemFormInstance>({
  validate,
  getFormData,
  memoryMedia,
  deletedMediaIds,
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