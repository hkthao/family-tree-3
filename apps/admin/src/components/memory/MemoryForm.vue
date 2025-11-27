<template>
  <v-stepper v-model="activeStep" :alt-labels="true" :hide-actions="true" flat>
    <v-stepper-header class="stepper-header">
      <!-- Step 1: Photo Upload -->
      <v-stepper-item :value="1" :title="t('memory.create.step1.title')" :complete="activeStep > 1"></v-stepper-item>
      <v-divider></v-divider>
      <!-- Step 2: General Information -->
      <v-stepper-item :value="2" :title="t('memory.create.step2.title')" :complete="activeStep > 2"></v-stepper-item>
      <v-divider></v-divider>
      <!-- Step 3: Review & Generate Story -->
      <v-stepper-item :value="3" :title="t('memory.create.step5.reviewAndSave')" :complete="activeStep > 3"></v-stepper-item>
    </v-stepper-header>

    <v-stepper-window>
      <!-- Step 1 Content: Photo Upload -->
      <v-stepper-window-item :value="1">
          <MemoryStep1PhotoUpload ref="step1Ref" v-model="internalMemory" :readonly="readonly" />
      </v-stepper-window-item>

      <!-- Step 2 Content: General Information -->
      <v-stepper-window-item :value="2">
        <MemoryStep2GeneralInfo
          ref="step2Ref"
          v-model="internalMemory"
          :readonly="readonly"
          :member-id="memberId"
        />
      </v-stepper-window-item>

      <!-- Step 3 Content: Review & Generate Story -->
      <v-stepper-window-item :value="3">
        <MemoryStep3ReviewSave
          ref="step3Ref"
          v-model="internalMemory"
          :readonly="readonly"
          @story-generated="handleStoryGenerated"
        />
      </v-stepper-window-item>
    </v-stepper-window>
  </v-stepper>
</template>

<script setup lang="ts">
import { ref, watch, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory'; // Only MemoryDto needed
import MemoryStep1PhotoUpload from './MemoryStep1PhotoUpload.vue';
import MemoryStep2GeneralInfo from './MemoryStep2GeneralInfo.vue';
import MemoryStep3ReviewSave from './MemoryStep3ReviewSave.vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null; // Prop to potentially pre-fill memberId
}>();

const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const activeStep = ref(1);

const step1Ref = ref<InstanceType<typeof MemoryStep1PhotoUpload> | null>(null);
const step2Ref = ref<InstanceType<typeof MemoryStep2GeneralInfo> & { validate: () => Promise<boolean>, triggerAiAnalysis: () => Promise<void> } | null>(null);
const step3Ref = ref<InstanceType<typeof MemoryStep3ReviewSave> & { generateStory: () => Promise<void>, generatedStory: Ref<string | null>, generatedTitle: Ref<string | null>, storyEditorValid: Ref<boolean> } | null>(null);

// Use a computed property for internal model to handle v-model
const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      title: model.title ?? null, // Default to null if undefined
      story: model.story ?? null, // Default to null if undefined
      rawInput: model.rawInput ?? '', // Default to '' if undefined
      storyStyle: model.storyStyle ?? 'nostalgic', // Default to 'nostalgic' if undefined
      perspective: model.perspective ?? 'firstPerson', // Default to 'firstPerson' if undefined
      eventSuggestion: model.eventSuggestion ?? null, // Default to null if undefined
      customEventDescription: model.customEventDescription ?? null, // Default to null if undefined
      emotionContextTags: model.emotionContextTags ?? [], // Default to [] if undefined
      // Ensure faces is an array to prevent .map() errors if it's null/undefined
      faces: model.faces ?? [],
      // photoAnalysisResult can be null, so no need to default to a specific value other than null
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

const handleStoryGenerated = (payload: { story: string | null; title: string | null }) => {
  internalMemory.value.story = payload.story;
  internalMemory.value.title = payload.title;
  // No automatic step advancement here, user clicks Next/Save
};

const validateStep = async (step: number) => {
  if (step === 1) {
    const hasDetectedFaces = (internalMemory.value.faces?.length ?? 0) > 0;
    const isPhotoProcessingValid = step1Ref.value?.isValid ?? false;
    const selectedTargetMemberFaceId = step1Ref.value?.selectedTargetMemberFaceId;
    const isTargetFaceSelected = selectedTargetMemberFaceId !== null && selectedTargetMemberFaceId !== undefined;

    if (!hasDetectedFaces) {
      showSnackbar(t('memory.validation.noFacesDetectedInPhoto'), 'warning');
      return false;
    }

    if (!isPhotoProcessingValid) {
      showSnackbar(t('memory.validation.photoProcessingNotDone'), 'warning');
      return false;
    }

    if (hasDetectedFaces && !isTargetFaceSelected) {
      showSnackbar(t('memory.validation.noTargetMemberSelected'), 'warning');
      return false;
    }

    return true; // All checks passed for step 1
  } else if (step === 2) {
    return step2Ref.value ? await step2Ref.value.validate() : false;
  } else if (step === 3) { // NEW: Validate Step 3 (Review & Generate Story)
    if (!internalMemory.value.story || internalMemory.value.story.trim() === '') {
      showSnackbar(t('memory.validation.noStoryGenerated'), 'warning');
      return false;
    }
    // If StoryEditor has internal validation, call it. For now, assume story generated is sufficient
    return step3Ref.value ? await step3Ref.value.storyEditorValid.value : true;
  }
  return true; // Default to valid for other steps
};

const nextStep = async () => {
  if (props.readonly) {
    if (activeStep.value < 3) activeStep.value++; // Max step is 3 now
    return;
  }

  const currentStep = activeStep.value;
  const isValid = await validateStep(currentStep);

  if (!isValid) {
    return; // Do not proceed if current step is not valid
  }

  // Max step is 3 now
  if (activeStep.value < 3) { // Only advance if not already at the last step
    activeStep.value++;
  } else {
    // If at the last step (3) and valid, then trigger submit for parent to save
    emit('submit');
  }
};

const prevStep = () => {
  if (activeStep.value > 1) {
    activeStep.value--;
  }
};

defineExpose({
  validateStep, // Re-expose validateStep
  activeStep,
  nextStep,
  prevStep,
  step1Ref, // Expose step1Ref for direct access to its properties
  step3Ref, // Expose step3Ref for direct access to its properties
});
</script>

<style scoped>
.stepper-header {
  box-shadow: none;
}
</style>