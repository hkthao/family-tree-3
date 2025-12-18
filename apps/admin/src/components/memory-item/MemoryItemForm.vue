<template>
  <v-form :disabled="props.readOnly" ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" v-if="mediaManagement.memoryMedia.value && mediaManagement.memoryMedia.value.length > 0">
        <v-carousel cycle hide-delimiter-background :continuous="false" hide-delimiters>
          <v-carousel-item v-for="(media, index) in mediaManagement.memoryMedia.value" :key="media.id || index">
            <div>
              <v-img :src="media.url" cover class="carousel-image"></v-img>
              <v-btn v-if="!props.readOnly" class="carousel-delete-btn" icon="mdi-delete" color="error" size="small"
                @click="mediaManagement.removeMedia(media)"></v-btn>
            </div>
          </v-carousel-item>
        </v-carousel>
      </v-col>
      <v-col v-if="!props.readOnly" cols="12">
        <VFileUpload :label="t('memoryItem.form.memoryMediaFile')" v-model="mediaManagement.uploadedFiles.value" :accept="mediaManagement.acceptedMimeTypes.value"
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
import { ref, computed, reactive, watch, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { MemoryItem } from '@/types';
import { EmotionalTag } from '@/types';
import { VDateInput } from 'vuetify/labs/VDateInput';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import { useMemoryMediaForm, type LocalMemoryMedia } from '@/composables';


interface LocalMemoryItem extends Omit<MemoryItem, 'persons' | 'memoryMedia' | 'deletedMediaIds'> {
  personIds: string[]; // Use personIds for autocomplete
}

interface MemoryItemFormProps {
  initialMemoryItemData?: MemoryItem;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<MemoryItemFormProps>();

const formRef = ref<VForm | null>(null);
const { t } = useI18n();

const defaultNewMemoryItem: LocalMemoryItem = {
  id: '',
  familyId: props.familyId,
  title: '',
  description: undefined,
  happenedAt: undefined,
  emotionalTag: EmotionalTag.Neutral,
  personIds: [],
  memoryPersons: [],
};

// Initialize form with local type
const form = reactive<LocalMemoryItem>(
  props.initialMemoryItemData
    ? {
      ...props.initialMemoryItemData,
      personIds: props.initialMemoryItemData.memoryPersons ? props.initialMemoryItemData.memoryPersons.map(p => p.memberId) : [],
    }
    : { ...defaultNewMemoryItem },
);

const mediaManagement = useMemoryMediaForm({
  initialMedia: props.initialMemoryItemData?.memoryMedia || [],
  memoryItemId: form.id,
  readOnly: props.readOnly || false,
});


watch(() => props.initialMemoryItemData, (newData) => {
  if (newData) {
    Object.assign(form, {
      ...newData,
      personIds: newData.memoryPersons ? newData.memoryPersons.map(p => p.memberId) : [],
    });
    // Update media in composable
    mediaManagement.memoryMedia.value = [...(newData.memoryMedia || [])];
    mediaManagement.deletedMediaIds.value = []; // Reset deleted media IDs on data change
  } else {
    Object.assign(form, { ...defaultNewMemoryItem });
    mediaManagement.memoryMedia.value = [];
    mediaManagement.deletedMediaIds.value = []; // Reset deleted media IDs on data change
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
  if (!formRef.value) return false;
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

export interface MemoryItemFormExpose {
  validate: () => Promise<boolean>;
  getFormData: () => MemoryItem;
  newlyUploadedFiles: ComputedRef<File[]>;
}

defineExpose<MemoryItemFormExpose>({
  validate,
  getFormData,
  newlyUploadedFiles: mediaManagement.newlyUploadedFiles,
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
  top: 5px;
  right: 5px;
}
</style>
