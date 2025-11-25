<template>
  <v-stepper v-model="activeStep" :alt-labels="true" :hide-actions="true" flat>
    <v-stepper-header class="stepper-header">
      <!-- Step 1: Photo Upload -->
      <v-stepper-item :value="1" :title="t('memory.create.step1.title')" :complete="activeStep > 1"></v-stepper-item>
      <v-divider></v-divider>
      <!-- Step 2: General Information -->
      <v-stepper-item :value="2" :title="t('memory.create.step2.title')" :complete="activeStep > 2"></v-stepper-item>
      <v-divider></v-divider>
      <!-- Step 3: Review & Save -->
      <v-stepper-item :value="3" :title="t('memory.create.step3.title')" :complete="activeStep > 3"></v-stepper-item>
    </v-stepper-header>

    <v-stepper-window>
      <!-- Step 1 Content: Photo Upload -->
      <v-stepper-window-item :value="1">
          <MemoryStep1PhotoUpload ref="step1Ref" v-model="internalMemory" :readonly="readonly" />
      </v-stepper-window-item>

      <!-- Step 2 Content: General Information -->
      <v-stepper-window-item :value="2">
        <MemoryStep2GeneralInfo ref="step2Ref" v-model="internalMemory" :readonly="readonly" :member-id="memberId" />
      </v-stepper-window-item>

      <!-- Step 3 Content: Review & Save -->
      <v-stepper-window-item :value="3">
        <MemoryStep3ReviewSave v-model="internalMemory" :readonly="readonly" />
      </v-stepper-window-item>
    </v-stepper-window>
  </v-stepper>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory';
import MemoryStep1PhotoUpload from './MemoryStep1PhotoUpload.vue';
import MemoryStep2GeneralInfo from './MemoryStep2GeneralInfo.vue';
import MemoryStep3ReviewSave from './MemoryStep3ReviewSave.vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null; // Prop to potentially pre-fill memberId
}>();

const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar(); // Initialize useGlobalSnackbar
const activeStep = ref(1);

const step1Ref = ref<InstanceType<typeof MemoryStep1PhotoUpload> | null>(null);
const step2Ref = ref<InstanceType<typeof MemoryStep2GeneralInfo> | null>(null);

// Use a computed property for internal model to handle v-model
const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      rawInput: model.rawInput ?? undefined,
      story: model.story ?? undefined,
      eventSuggestion: model.eventSuggestion ?? undefined,
      customEventDescription: model.customEventDescription ?? undefined,
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined,
      faces: model.faces ?? [],
    } as MemoryDto;
  },
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

// If memberId prop is provided and internalMemory.memberId is empty, pre-fill it
watch(() => props.memberId, (newMemberId) => {
  if (newMemberId && !internalMemory.value.memberId) {
    internalMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

const validateStep = async (step: number) => {
  if (step === 1) {
    // Step 1 doesn't have explicit form validation, but check if photo processing is done
    const isValid = step1Ref.value?.isValid;
    const isMainCharacterSelected = step1Ref.value?.selectedMainCharacterFaceId !== null && step1Ref.value?.selectedMainCharacterFaceId !== undefined;
    const hasDetectedFaces = step1Ref.value?.memoryFaceStore?.detectedFaces ? step1Ref.value.memoryFaceStore.detectedFaces.length > 0 : false;

    if (!isValid) {
      showSnackbar(t('memory.validation.photoProcessingNotDone'), 'warning'); // Need to add this translation
      return false;
    }
    if (!isMainCharacterSelected && hasDetectedFaces) {
      showSnackbar(t('memory.validation.noMainCharacterSelected'), 'warning'); // Need to add this translation
      return false;
    }

    return isValid && (isMainCharacterSelected || !hasDetectedFaces);
  } else if (step === 2) {
    return step2Ref.value ? await step2Ref.value.validate() : false;
  }
  return true; // Step 3 is review, no direct validation form
};

const nextStep = async () => {
  if (props.readonly) {
    activeStep.value++;
    return;
  }
  const currentStep = activeStep.value;
  const isValid = await validateStep(currentStep);

  if (isValid) {
    activeStep.value++;
  }
};

const prevStep = () => {
  if (activeStep.value > 1) {
    activeStep.value--;
  }
};

// Expose validate function and selectedFiles to parent component
defineExpose({
  validateStep,
  activeStep,
  nextStep,
  prevStep,
  step1Ref, // Expose step1Ref for direct access to its properties
});
</script>

<style scoped>
.stepper-header {
  box-shadow: none;
}
</style>