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
import type { MemoryDto } from '@/types/memory'; // Only MemoryDto needed
import type { AiPhotoAnalysisInputDto } from '@/types/ai'; // Only AiPhotoAnalysisInputDto needed
import type { MemoryFaceState } from '@/stores/memory.store'; // NEW IMPORT
import MemoryStep1PhotoUpload from './MemoryStep1PhotoUpload.vue';
import MemoryStep2GeneralInfo from './MemoryStep2GeneralInfo.vue';
import MemoryStep3ReviewSave from './MemoryStep3ReviewSave.vue';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import { useMemoryStore } from '@/stores/memory.store'; // NEW IMPORT

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null; // Prop to potentially pre-fill memberId
}>();

const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const memoryStore = useMemoryStore(); // Initialize memory store
const activeStep = ref(1);

const step1Ref = ref<InstanceType<typeof MemoryStep1PhotoUpload> | null>(null);
const step2Ref = ref<InstanceType<typeof MemoryStep2GeneralInfo> | null>(null);

// Use a computed property for internal model to handle v-model
const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      // Ensure faces is an array to prevent .map() errors if it's null/undefined
      faces: model.faces ?? [],
      emotionContextTags: model.emotionContextTags ?? [],
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
  }
  return true; // Step 3 (review) always valid for direct progression
};

// Helper function to build AiPhotoAnalysisInputDto
const buildAiPhotoAnalysisInput = (
  memory: MemoryDto,
  faceRecognitionState: MemoryFaceState // Pass the entire faceRecognition state
): AiPhotoAnalysisInputDto => {
  const aiInput: AiPhotoAnalysisInputDto = {
    imageUrl: faceRecognitionState.resizedImageUrl || memory.photoUrl, // Use resized URL if available
    imageBase64: faceRecognitionState.resizedImageUrl ? undefined : memory.photo, // Use base64 only if no resized URL
    imageSize: memory.imageSize || '512x512',
    exif: memory.exifData,
    targetFaceId: memory.targetFaceId,
    targetFaceCropUrl: null, // Placeholder for now, can be generated later if needed

    faces: memory.faces!.map(face => ({
      faceId: face.id,
      bbox: [face.boundingBox.x, face.boundingBox.y, face.boundingBox.width, face.boundingBox.height],
      emotionLocal: {
        dominant: face.emotion || '',
        confidence: face.emotionConfidence || 0,
      },
      quality: face.quality || 'unknown',
    })),
    memberInfo: undefined,
    otherFacesSummary: [],
  };

  const targetMemberFace = memory.faces!.find(
    (face) => face.id === aiInput.targetFaceId
  );

  if (targetMemberFace && targetMemberFace.memberId) {
    aiInput.memberInfo = {
      id: targetMemberFace.memberId,
      name: targetMemberFace.memberName,
      age: targetMemberFace.birthYear ? (new Date().getFullYear() - targetMemberFace.birthYear) : undefined,
    };
  }

  aiInput.otherFacesSummary = memory.faces!
    .filter(face => face.id !== aiInput.targetFaceId)
    .map(face => ({
      emotionLocal: face.emotion || '',
    }));

  return aiInput;
};

const nextStep = async () => {
  if (props.readonly) {
    activeStep.value++;
    return;
  }

  const currentStep = activeStep.value;
  const isValid = await validateStep(currentStep);

  if (!isValid) {
    return; // Do not proceed if current step is not valid
  }

  if (currentStep === 1) {
    const aiInput = buildAiPhotoAnalysisInput(internalMemory.value, memoryStore.faceRecognition);
    const aiResult = await memoryStore.services.ai.analyzePhoto({ Input: aiInput });

    if (aiResult.ok) {
      internalMemory.value.photoAnalysisResult = aiResult.value;
      activeStep.value++; // Proceed to next step only on success
    } else {
      showSnackbar(
        aiResult.error?.message || t('memory.errors.aiAnalysisFailed'),
        'error'
      );
      // Do not proceed to next step
    }
  } else {
    // For other steps, just advance
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