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
      rawInput: model.rawInput ?? undefined,
      story: model.story ?? undefined,
      eventSuggestion: model.eventSuggestion ?? undefined,
      customEventDescription: model.customEventDescription ?? undefined,
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined,
      faces: model.faces ?? [],
      // Ensure photoAnalysisResult is initialized
      photoAnalysisResult: model.photoAnalysisResult ?? null,
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
    const hasPhoto = internalMemory.value.photo !== null && internalMemory.value.photo !== undefined && internalMemory.value.photo !== '';
    const hasDetectedFaces = internalMemory.value.faces && internalMemory.value.faces.length > 0;
    const isValid = step1Ref.value?.isValid; // This seems to check if photo processing is finished
    // Ensure selectedTargetMemberFaceId is correctly mapped from exposed property
    const isTargetMemberSelected = step1Ref.value?.selectedTargetMemberFaceId !== null && step1Ref.value?.selectedTargetMemberFaceId !== undefined;

    if (!hasPhoto) {
      showSnackbar(t('memory.validation.noPhotoUploaded'), 'warning');
      return false;
    }

    if (!hasDetectedFaces) {
      showSnackbar(t('memory.validation.noFacesDetectedInPhoto'), 'warning');
      return false;
    }

    if (!isValid) {
      showSnackbar(t('memory.validation.photoProcessingNotDone'), 'warning');
      return false;
    }
    // This check is only relevant if there ARE detected faces
    if (!isTargetMemberSelected && hasDetectedFaces) { // Re-added hasDetectedFaces check for clarity
      showSnackbar(t('memory.validation.noTargetMemberSelected'), 'warning');
      return false;
    }

    return hasPhoto && hasDetectedFaces && isValid && (isTargetMemberSelected || !hasDetectedFaces);
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
    if (currentStep === 1) {
      // Construct AiPhotoAnalysisInputDto
      const aiInput: AiPhotoAnalysisInputDto = {
        imageUrl: internalMemory.value.photoUrl,
        imageBase64: internalMemory.value.photo, // This is the base64 string
        imageSize: internalMemory.value.imageSize || '512x512', // Use stored size or default
        exif: internalMemory.value.exifData,
        targetFaceId: internalMemory.value.targetFaceId, // Already added to MemoryDto
        targetFaceCropUrl: null, // Placeholder for now, can be generated later if needed

        faces: internalMemory.value.faces!.map(face => ({ // Added non-null assertion
          faceId: face.id,
          bbox: [face.boundingBox.x, face.boundingBox.y, face.boundingBox.width, face.boundingBox.height],
          emotionLocal: {
            dominant: face.emotion || '',
            confidence: face.emotionConfidence || 0,
          },
          quality: face.quality || 'unknown',
        })),
        
        memberInfo: undefined, // Default
        otherFacesSummary: [], // Default
      };

      // Find the selected main character face from detected faces
      const targetMemberFace = internalMemory.value.faces!.find( // Added non-null assertion
        (face) => face.id === aiInput.targetFaceId
      );

      if (targetMemberFace && targetMemberFace.memberId) {
        // Populate with available info. Gender is not directly in DetectedFace/MemoryFaceDto.
        // It should be fetched or added to DetectedFace type if possible.
        aiInput.memberInfo = {
          id: targetMemberFace.memberId,
          name: targetMemberFace.memberName,
          age: targetMemberFace.birthYear ? (new Date().getFullYear() - targetMemberFace.birthYear) : undefined,
          // gender: targetMemberFace.gender, // Gender is not directly available here
        };
      }

      // Construct otherFacesSummary
      aiInput.otherFacesSummary = internalMemory.value.faces! // Added non-null assertion
        .filter(face => face.id !== aiInput.targetFaceId)
        .map(face => ({
          emotionLocal: face.emotion || '',
        }));
      
      // Call AI analysis action
      // memoryStore.aiAnalysis.loading is automatically managed by the action
      const aiResult = await memoryStore.services.ai.analyzePhoto({ Input: aiInput }); // WRAP aiInput IN { Input: ... }
      
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