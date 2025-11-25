<template>
  <v-stepper v-model="activeStep" :alt-labels="!readonly" :hide-actions="true" flat>
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
        <FaceUploadInput ref="faceUploadInputRef" @file-uploaded="handleFileUpload" />
        <v-progress-linear v-if="faceStore.loading" indeterminate color="primary" class="my-4"></v-progress-linear>
        <div v-if="faceStore.uploadedImage && faceStore.detectedFaces.length > 0" class="mt-4">
          <v-row>
            <v-col cols="12" md="8">
              <FaceBoundingBoxViewer :image-src="faceStore.uploadedImage" :faces="faceStore.detectedFaces" />
            </v-col>
            <v-col cols="12" md="4">
              <FaceDetectionSidebar :faces="faceStore.detectedFaces" />
            </v-col>
          </v-row>
        </div>
        <v-alert v-else-if="!faceStore.loading && !faceStore.uploadedImage" type="info" class="my-4">{{
          t('face.recognition.uploadPrompt') }}</v-alert>
        <v-alert v-else-if="!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0"
          type="info" class="my-4">{{ t('face.recognition.noFacesDetected') }}</v-alert>
      </v-stepper-window-item>

      <!-- Step 2 Content: General Information -->
      <v-stepper-window-item :value="2">
        <v-form ref="formStep2">
          <v-row>
            <v-col cols="12">
              <MemberAutocomplete v-model="internalMemory.memberId" :label="t('member.form.member')"
                :rules="readonly ? [] : [(v: string) => !!v || t('common.validations.required')]"
                :readonly="readonly || !!memberId" required></MemberAutocomplete>
            </v-col>

            <v-col cols="12">
              <AiSuggestionsForm v-model="internalMemory" :readonly="readonly" />
            </v-col>

            <v-col cols="12">
              <v-text-field v-model="internalMemory.title" :label="t('memory.storyEditor.title')"
                :rules="readonly ? [] : [(v: string) => !!v || t('common.validations.required')]" :readonly="readonly"
                required></v-text-field>
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="internalMemory.rawInput" :label="t('memory.create.rawInputPlaceholder')"
                :readonly="readonly"></v-textarea>
            </v-col>
            <v-col cols="12" v-if="internalMemory.story">
              <v-textarea v-model="internalMemory.story" :label="t('memory.storyEditor.storyContent')"
                readonly></v-textarea>
            </v-col>
            <v-col cols="12">
              <v-combobox v-model="internalMemory.tags" :label="t('memory.storyEditor.tags')" chips multiple clearable
                :readonly="readonly"></v-combobox>
            </v-col>
            <v-col cols="12">
              <v-combobox v-model="internalMemory.keywords" :label="t('memory.storyEditor.keywords')" chips multiple
                clearable :readonly="readonly"></v-combobox>
            </v-col>
          </v-row>
        </v-form>
      </v-stepper-window-item>

      <!-- Step 3 Content: Review & Save -->
      <v-stepper-window-item :value="3">
        <v-row>
          <v-col cols="12">
            <h4>{{ t('memory.create.step2.title') }}</h4> <!-- General Info Title for review -->
            <p><strong>{{ t('member.form.member') }}:</strong> {{ internalMemory.memberId }}</p>
            <p><strong>{{ t('memory.storyEditor.title') }}:</strong> {{ internalMemory.title }}</p>
            <p v-if="internalMemory.rawInput"><strong>{{ t('memory.create.rawInputPlaceholder') }}:</strong> {{ internalMemory.rawInput }}</p>
            <p v-if="internalMemory.story"><strong>{{ t('memory.storyEditor.storyContent') }}:</strong> {{ internalMemory.story }}</p>
            <p v-if="internalMemory.tags && internalMemory.tags.length > 0">
              <strong>{{ t('memory.storyEditor.tags') }}:</strong> {{ internalMemory.tags.join(', ') }}
            </p>
            <p v-if="internalMemory.keywords && internalMemory.keywords.length > 0">
              <strong>{{ t('memory.storyEditor.keywords') }}:</strong> {{ internalMemory.keywords.join(', ') }}
            </p>

            <!-- AI Event Suggestion Review -->
            <p v-if="internalMemory.eventSuggestion">
              <strong>{{ t('memory.create.aiEventSuggestion.title') }}:</strong> {{ internalMemory.eventSuggestion }}
              <span v-if="internalMemory.customEventDescription && internalMemory.eventSuggestion === 'unsure'">
                ({{ internalMemory.customEventDescription }})
              </span>
            </p>

            <!-- AI Character Suggestions Review -->
            <p v-if="internalMemory.faces && internalMemory.faces.length > 0">
              <strong>{{ t('memory.create.aiCharacterSuggestion.title') }}:</strong>
              <v-list density="compact">
                <v-list-item v-for="(face, index) in internalMemory.faces" :key="index">
                  <v-list-item-title>
                    {{ t('member.form.member') }}: {{ face.memberId || t('common.unknown') }}
                  </v-list-item-title>
                  <v-list-item-subtitle v-if="face.relationPrompt">
                    {{ t('memory.create.aiCharacterSuggestion.relationPrompt') }}: {{ face.relationPrompt }}
                  </v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </p>

            <!-- AI Emotion & Context Suggestions Review -->
            <p v-if="internalMemory.emotionContextTags && internalMemory.emotionContextTags.length > 0">
              <strong>{{ t('memory.create.aiEmotionContextSuggestion.title') }}:</strong> {{
                internalMemory.emotionContextTags.join(', ') }}
            </p>
            <p v-if="internalMemory.customEmotionContext">
              <strong>{{ t('memory.create.aiEmotionContextSuggestion.customInput') }}:</strong> {{
                internalMemory.customEmotionContext }}
            </p>

          </v-col>
          <v-col cols="12" v-if="faceStore.uploadedImage">
            <h4>{{ t('memory.create.step1.title') }}</h4> <!-- Photo Upload Title for review -->
            <img :src="faceStore.uploadedImage" height="100" class="ma-2" />
            <p v-if="!props.readonly">{{ t('memory.create.step2.analysisResult') }}</p>
          </v-col>
        </v-row>
      </v-stepper-window-item>
    </v-stepper-window>
  </v-stepper>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberAutocomplete } from '@/components/common';
import type { MemoryDto } from '@/types/memory';
import { VFileUpload } from 'vuetify/labs/VFileUpload'
import AiSuggestionsForm from './AiSuggestionsForm.vue';
import { useFaceStore } from '@/stores/face.store'; // Import useFaceStore
import { FaceUploadInput, FaceBoundingBoxViewer, FaceDetectionSidebar } from '@/components/face'; // Import face components
import type { DetectedFace } from '@/types'; // Import DetectedFace type
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string; // Prop to potentially pre-fill memberId
}>();

const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);

const { t } = useI18n();
const faceStore = useFaceStore(); // Khởi tạo faceStore
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar
const formStep1 = ref<HTMLFormElement | null>(null); // Now for Photo Upload
const formStep2 = ref<HTMLFormElement | null>(null); // Now for General Info
const activeStep = ref(1);
const MAX_FILES = 5;
const MAX_FILE_SIZE_MB = 2; // 2 MB
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
const selectedFiles = ref<File[]>([]);
const faceUploadInputRef = ref<InstanceType<typeof FaceUploadInput> | null>(null); // Ref for FaceUploadInput

const photoRules = [
  (files: File[]) => {
    if (!files || files.length === 0) return true;
    if (files.length > MAX_FILES) {
      return t('memory.validation.maxPhotos', { max: MAX_FILES });
    }
    for (const file of files) {
      if (file.size > MAX_FILE_SIZE_BYTES) {
        return t('memory.validation.maxPhotoSize', { max: MAX_FILE_SIZE_MB });
      }
    }
    return true;
  },
];

// Use a computed property for internal model to handle v-model
const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      rawInput: model.rawInput ?? undefined, // Use rawInput instead of story
      story: model.story ?? undefined, // Add story
      eventSuggestion: model.eventSuggestion ?? undefined, // Changed null to undefined
      customEventDescription: model.customEventDescription ?? undefined, // Changed null to undefined
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined, // Changed null to undefined
      faces: model.faces ?? [],
    } as MemoryDto; // Explicitly cast the final object to the expected type
  },
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

// If memberId prop is provided and internalMemory.memberId is empty, pre-fill it
watch(() => props.memberId, (newMemberId) => {
  if (newMemberId && !internalMemory.value.memberId) {
    internalMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

// Emit selectedFiles changes to parent
watch(selectedFiles, (newFiles) => {
  emit('update:selectedFiles', newFiles);
});

watch(() => faceStore.error, (newError) => {
  if (newError) {
    showSnackbar(newError, 'error');
  }
});

const validateStep = async (step: number) => {
  if (step === 1) { // Now validates Photo Upload
    return formStep1.value ? (await formStep1.value.validate()).valid : false;
  } else if (step === 2) { // Now validates General Info
    return formStep2.value ? (await formStep2.value.validate()).valid : false;
  }
  return true; // Step 3 is review, no direct validation form
};

const handleFileUpload = async (file: File | File[] | null) => {
  if (file instanceof File) {
    await faceStore.detectFaces(file);
    if (faceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = faceStore.detectedFaces;
      internalMemory.value.photoUrl = faceStore.uploadedImage;
    } else if (!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else if (Array.isArray(file) && file.length > 0) {
    await faceStore.detectFaces(file[0]);
    if (faceStore.detectedFaces.length > 0) {
      internalMemory.value.photoAnalysisId = 'generated_id'; // Placeholder for analysis ID
      internalMemory.value.faces = faceStore.detectedFaces;
      internalMemory.value.photoUrl = faceStore.uploadedImage;
    } else if (!faceStore.loading && faceStore.uploadedImage && faceStore.detectedFaces.length === 0) {
      showSnackbar(t('face.recognition.noFacesDetected'), 'info');
    }
  } else {
    faceStore.resetState();
    internalMemory.value.photoAnalysisId = undefined;
    internalMemory.value.faces = [];
    internalMemory.value.photoUrl = undefined;
  }
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
      if (faceStore.uploadedImage && faceStore.detectedFaces.length > 0) {
        activeStep.value = 2;
      } else if (faceStore.uploadedImage && faceStore.detectedFaces.length === 0) {
        // If an image was uploaded but no faces detected, still allow to proceed to step 2
        activeStep.value = 2;
      } else {
        // No image uploaded, directly go to step 2
        activeStep.value = 2;
      }
    } else if (currentStep === 2) {
      activeStep.value = 3;
    }
  }
};

const prevStep = () => {
  if (activeStep.value > 1) {
    activeStep.value--;
  }
};

const analyzePhotos = () => {
  // This function is now deprecated. Logic moved to handleFileUpload and useFaceStore.
};

const fileToBase64 = (file: File): string => {
  if (!file) return '';
  return URL.createObjectURL(file);
};

// Expose validate function and selectedFiles to parent component
defineExpose({
  validateStep,
  selectedFiles,
  activeStep,
  nextStep,
  prevStep,
});
</script>

<style scoped>
.stepper-header {
  box-shadow: none;
}
</style>