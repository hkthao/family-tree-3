<template>
  <v-container fluid>
    <v-img v-if="imagePreviewUrl" :src="imagePreviewUrl" :alt="urlFormData.fileName || 'Image preview'"
      class="my-4 rounded" contain></v-img>
    <v-form ref="urlForm" @submit.prevent="submitUrl">
      <v-row>
        <v-col cols="12">
          <v-text-field v-model="urlFormData.url" :label="t('familyMedia.addLink.form.url')"
            :rules="[rules.required, rules.url]" required prepend-inner-icon="mdi-link"></v-text-field>
        </v-col>
        <v-col cols="12">
          <v-text-field v-model="urlFormData.fileName" :label="t('familyMedia.addLink.form.fileName')"
            :rules="[rules.required]" required prepend-inner-icon="mdi-file"></v-text-field>
        </v-col>
        <v-col cols="12">
          <v-select v-model="urlFormData.mediaType" :label="t('familyMedia.addLink.form.mediaType')"
            :items="mediaTypeOptions" item-title="title" item-value="value" clearable
            prepend-inner-icon="mdi-file-image"></v-select>
        </v-col>
        <v-col cols="12">
          <v-textarea v-model="urlFormData.description" :label="t('familyMedia.addLink.form.description')"
            rows="3" prepend-inner-icon="mdi-text-box-outline"></v-textarea>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="d-flex justify-end">
          <v-btn color="primary" @click="submitUrl" :loading="isAddingUrl">{{ t('common.save') }}</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { useValidationRules } from '@/composables/utils/useValidationRules';
import { useFamilyMediaAddLinkLogic } from '@/composables/family-media/logic/useFamilyMediaAddLinkLogic'; // Updated import path

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'saved']);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { rules } = useValidationRules();

const {
  state: { urlFormData, mediaTypeOptions, isAddingUrl, imagePreviewUrl }, // Added imagePreviewUrl
  actions: { submitUrl },
} = useFamilyMediaAddLinkLogic({
  familyId: props.familyId,
  t,
  showSnackbar,
  rules,
  emit: (event: 'close' | 'saved') => emit(event),
});
</script>