<template>
  <v-form ref="formRef">
    <v-alert
      type="warning"
      variant="tonal"
      class="mb-4"
    >
      {{ t('imageRestorationJob.claim.alertMessage') }}
    </v-alert>
    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="jobData.originalImageUrl"
          :label="t('imageRestorationJob.form.originalImageUrl')"
          :rules="[rules.required, rules.url]"
          :readonly="readOnly"
          data-testid="original-image-url-field"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row v-if="readOnly && jobData.restoredImageUrl">
      <v-col cols="12">
        <v-text-field
          v-model="jobData.restoredImageUrl"
          :label="t('imageRestorationJob.form.restoredImageUrl')"
          readonly
          data-testid="restored-image-url-field"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row v-if="readOnly && jobData.status">
      <v-col cols="12">
        <v-text-field
          v-model="jobData.status"
          :label="t('imageRestorationJob.form.status')"
          readonly
          data-testid="status-field"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row v-if="readOnly && jobData.errorMessage">
      <v-col cols="12">
        <v-text-field
          v-model="jobData.errorMessage"
          :label="t('imageRestorationJob.form.errorMessage')"
          readonly
          data-testid="error-message-field"
        ></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, reactive, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from '@/composables/validation/useRules'; // Assuming a useRules composable for common validation rules

// Define the shape of the ImageRestorationJob data
export interface ImageRestorationJobFormData {
  originalImageUrl: string;
  familyId: string;
  restoredImageUrl?: string;
  status?: string; // Assuming status is string for display
  errorMessage?: string;
}

// Define the interface for the form instance, used for ref
export interface IImageRestorationJobFormInstance {
  validate: () => Promise<{ valid: boolean }>;
  reset: () => void;
  getFormData: () => ImageRestorationJobFormData;
}

const props = defineProps({
  initialData: {
    type: Object as PropType<ImageRestorationJobFormData>,
    default: () => ({ originalImageUrl: '', familyId: '' }),
  },
  familyId: {
    type: String as PropType<string>,
    required: true,
  },
  readOnly: {
    type: Boolean,
    default: false,
  },
});

const { t } = useI18n();
const { rules } = useRules();
const formRef = ref<HTMLFormElement | null>(null);

const jobData = reactive<ImageRestorationJobFormData>({
  ...props.initialData,
  familyId: props.familyId, // Ensure familyId is always from props
});

watch(
  () => props.initialData,
  (newVal) => {
    Object.assign(jobData, newVal);
    jobData.familyId = props.familyId;
  },
  { deep: true }
);

// Expose methods for parent component to call
defineExpose<IImageRestorationJobFormInstance>({
  validate: async () => {
    const { valid } = await formRef.value!.validate();
    return { valid };
  },
  reset: () => {
    formRef.value!.reset();
    Object.assign(jobData, { originalImageUrl: '', familyId: props.familyId });
  },
  getFormData: () => ({ ...jobData }),
});
</script>

<style scoped></style>