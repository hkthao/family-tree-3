<template>
  <v-card :elevation="0" data-testid="image-restoration-job-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('imageRestorationJob.form.addTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="formRef" @submit.prevent>
        <VFileUpload
          v-model="file"
          :label="t('imageRestorationJob.form.imageFileLabel')"
          prepend-icon="mdi-camera"
          accept="image/*"
          :rules="[rules.required]"
          show-size
          counter
          data-testid="file-input"
        />
        <v-checkbox
          v-model="useCodeformer"
          :label="t('imageRestorationJob.form.useCodeformerLabel')"
          data-testid="use-codeformer-checkbox"
        />
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm"
        :disabled="isAddingImageRestorationJob">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem"
        :loading="isAddingImageRestorationJob" :disabled="isAddingImageRestorationJob">{{ t('common.save') }}</v-btn>
    </v-card-actions>
    <v-overlay
      :model-value="isAddingImageRestorationJob"
      class="align-center justify-center"
      persistent
      data-testid="loading-overlay"
    >
      <v-progress-circular
        color="primary"
        indeterminate
        size="64"
      ></v-progress-circular>
    </v-overlay>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { useImageRestorationJobAdd } from '@/composables';


const props = defineProps({
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();

const file = ref<File | undefined>(undefined);
const useCodeformer = ref<boolean>(false);
const formRef = ref<any>(null); // Ref for VForm validation

const rules = {
  required: (value: any) => !!value || t('common.required'),
};

const {
  state: { isAddingImageRestorationJob },
  actions: { closeForm },
  internal: { handleAddItem: composableHandleAddItem }, // Renamed to avoid conflict
} = useImageRestorationJobAdd({
  familyId: props.familyId,
  onSaveSuccess: () => {
    emit('close');
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
});

const handleAddItem = async () => {
  const { valid } = await formRef.value.validate();
  if (!valid) {
    return;
  }
  if (!file.value) {
    // This case should ideally be caught by rules.required on v-file-input,
    // but as a fallback, we can add a log or specific error message.
    console.error('No file selected.');
    return;
  }
  await composableHandleAddItem(file.value, useCodeformer.value);
};
</script>

<style scoped></style>