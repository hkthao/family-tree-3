<template>
  <v-form ref="formRef">
    <v-row>
      <v-col v-if="!readOnly" cols="12">
        <VFileUpload v-model="file" :label="t('familyMedia.form.fileLabel')" prepend-icon="mdi-camera"
          accept="image/*,video/*,audio/*,application/pdf" show-size :rules="readOnly ? [] : formRules.file"
          :disabled="readOnly"></VFileUpload>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="description" :label="t('familyMedia.form.descriptionLabel')" rows="3" clearable
          :disabled="readOnly"></v-textarea>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <!-- Display current media if in edit/detail mode -->
        <div cl v-if="initialMedia && initialMedia.filePath">
          <p class="text-subtitle-2 my-4">{{ t('familyMedia.form.currentMedia') }}</p>
          <v-img v-if="initialMedia.mediaType === MediaType.Image" :src="initialMedia.filePath" max-width="200"></v-img>
          <video v-else-if="initialMedia.mediaType === MediaType.Video" controls :src="initialMedia.filePath"
            max-width="200"></video>
          <audio v-else-if="initialMedia.mediaType === MediaType.Audio" controls :src="initialMedia.filePath"></audio>
          <p v-else-if="initialMedia.mediaType === MediaType.Document">
            <a :href="initialMedia.filePath" target="_blank">{{ initialMedia.fileName }}</a>
          </p>
        </div>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import type { FamilyMedia } from '@/types';
import { MediaType } from '@/types/enums';
import { VFileUpload } from 'vuetify/labs/VFileUpload';
import { useFamilyMediaForm } from '@/composables/family-media/useFamilyMediaForm';

interface FamilyMediaFormProps {
  initialMedia?: FamilyMedia;
  readOnly?: boolean;
}

const props = defineProps<FamilyMediaFormProps>();
defineEmits(['update:modelValue']);
const { t } = useI18n();

const {
  state: { formRef, file, description, formRules },
  actions: { getFormData, validate, resetValidation, resetForm },
} = useFamilyMediaForm(props.initialMedia);

defineExpose({
  getFormData,
  validate,
  resetValidation,
  resetForm,
});
</script>