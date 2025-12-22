<template>
  <v-card flat>
    <v-tabs v-model="tab" color="primary" align-tabs="center">
      <v-tab :value="0">{{ t('familyMedia.addUnified.uploadFileTabTitle') }}</v-tab>
      <v-tab :value="1">{{ t('familyMedia.addUnified.fromUrlTabTitle') }}</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item :value="0">
        <!-- Content for Upload File tab will go here -->
        <v-card :elevation="0">
          <v-progress-linear v-if="isSubmittingFile" indeterminate color="primary"></v-progress-linear>
          <v-card-text>
            <FamilyMediaForm ref="familyMediaFormRef" />
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="handleSubmitFile" :loading="isSubmittingFile">{{
              t('common.save') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-window-item>

      <v-window-item :value="1">
        <!-- Content for From URL tab will go here -->
        <v-container fluid>
          <v-form ref="urlForm" @submit.prevent="submitUrl">
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="urlFormData.url"
                  :label="t('familyMedia.addLink.form.url')"
                  :rules="[rules.required, rules.url]"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-text-field
                  v-model="urlFormData.fileName"
                  :label="t('familyMedia.addLink.form.fileName')"
                  :rules="[rules.required]"
                  required
                ></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-select
                  v-model="urlFormData.mediaType"
                  :label="t('familyMedia.addLink.form.mediaType')"
                  :items="mediaTypeOptions"
                  item-title="title"
                  item-value="value"
                  clearable
                ></v-select>
              </v-col>
              <v-col cols="12">
                <v-textarea
                  v-model="urlFormData.description"
                  :label="t('familyMedia.addLink.form.description')"
                  rows="3"
                ></v-textarea>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" class="d-flex justify-end">
                <v-btn color="primary" @click="submitUrl" :loading="isAddingUrl">{{ t('common.add') }}</v-btn>
              </v-col>
            </v-row>
          </v-form>
        </v-container>
      </v-window-item>
    </v-window>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media'; // Import FamilyMediaForm
import { useAddFamilyMediaMutation, useFamilyMediaFormLogic, useGlobalSnackbar } from '@/composables'; // Import for file upload logic
import { useValidationRules } from '@/composables/utils/useValidationRules'; // For URL validation
import { MediaType } from '@/types/enums'; // For media types
import { useAddFamilyMediaFromUrlMutation } from '@/composables/family-media/mutations/useAddFamilyMediaFromUrlMutation'; // For URL media
import { getMediaTypeOptions } from '@/composables/utils/mediaTypeOptions'; // For media type options

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const tab = ref(0);
const { showSnackbar } = useGlobalSnackbar();
const { rules } = useValidationRules();

// Logic for File Upload tab (from FamilyMediaAddView.vue)
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const familyIdRef = toRef(props, 'familyId');
const { mutateAsync: addFamilyMediaMutation } = useAddFamilyMediaMutation();

const {
  state: { isSubmitting: isSubmittingFile },
  actions: { handleSubmit: handleSubmitFile },
} = useFamilyMediaFormLogic({
  familyId: familyIdRef,
  mutation: addFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.addSuccess',
  errorMessageKey: 'familyMedia.messages.saveError',
  formRef: familyMediaFormRef,
  onSuccess: () => {
    showSnackbar(t('familyMedia.messages.addSuccess'), 'success'); // Adding snackbar for consistency
    emit('saved');
  },
  transformData: (data, familyId) => ({
    familyId,
    file: data.file,
    description: data.description,
  }),
});

// Logic for URL tab (from FamilyMediaAddLinkView.vue)
const urlForm = ref<HTMLFormElement | null>(null);
const urlFormData = ref({
  url: '',
  fileName: '',
  mediaType: undefined as MediaType | undefined,
  description: '',
});

const mediaTypeOptions = getMediaTypeOptions(t);

const { mutate: addFamilyMediaFromUrl, isPending: isAddingUrl } = useAddFamilyMediaFromUrlMutation();

const submitUrl = async () => {
  if (!urlForm.value) return;
  const { valid } = await urlForm.value.validate();
  if (valid) {
    const payload = {
      familyId: props.familyId,
      url: urlFormData.value.url,
      fileName: urlFormData.value.fileName,
      mediaType: urlFormData.value.mediaType,
      description: urlFormData.value.description,
    };
    addFamilyMediaFromUrl(payload, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.addLink.messages.success'), 'success');
        emit('saved');
        // Clear form after successful upload
        urlFormData.value.url = '';
        urlFormData.value.fileName = '';
        urlFormData.value.mediaType = undefined;
        urlFormData.value.description = '';
        urlForm.value?.resetValidation();
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.addLink.messages.error'), 'error');
      },
    });
  }
};
</script>

<style scoped>
/* Add any specific styles for FamilyMediaAddUnifiedView here */
</style>
