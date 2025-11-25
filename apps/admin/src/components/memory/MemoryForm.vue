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
        <v-form ref="formStep1">
          <v-row>
            <v-col cols="12">
              <VFileUpload v-model="selectedFiles" :label="t('memory.create.step1.choosePhoto')" :readonly="readonly"
                accept="image/*" show-size multiple max-files="5" :rules="readonly ? [] : photoRules" />
            </v-col>
          </v-row>
        </v-form>
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
          <v-col cols="12" v-if="selectedFiles.length > 0">
            <h4>{{ t('memory.create.step1.title') }}</h4> <!-- Photo Upload Title for review -->
            <p>{{ t('memory.create.step2.analysisResult') }}:</p>
            <!-- Placeholder for displaying analysis results -->
            <div v-for="(file, index) in selectedFiles" :key="index">
              <img :src="fileToBase64(file)" height="100" class="ma-2" />
              <span>{{ file.name }} ({{ (file.size / 1024 / 1024).toFixed(2) }} MB)</span>
            </div>
            <p v-if="!props.readonly">{{ t('memory.create.step2.noAnalysisYet') }}</p>
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

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string; // Prop to potentially pre-fill memberId
}>();

const emit = defineEmits(['update:modelValue', 'submit', 'update:selectedFiles']);

const { t } = useI18n();
const formStep1 = ref<HTMLFormElement | null>(null); // Now for Photo Upload
const formStep2 = ref<HTMLFormElement | null>(null); // Now for General Info
const activeStep = ref(1);
const MAX_FILES = 5;
const MAX_FILE_SIZE_MB = 2; // 2 MB
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;
const selectedFiles = ref<File[]>([]);
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

const validateStep = async (step: number) => {
  if (step === 1) { // Now validates Photo Upload
    return formStep1.value ? (await formStep1.value.validate()).valid : false;
  } else if (step === 2) { // Now validates General Info
    return formStep2.value ? (await formStep2.value.validate()).valid : false;
  }
  return true; // Step 3 is review, no direct validation form
};

const nextStep = async () => {
  if (props.readonly) { // For readonly mode, just advance step
    activeStep.value++;
    return;
  }
  const currentStep = activeStep.value;
  const isValid = await validateStep(currentStep);

  if (isValid) {
    if (currentStep === 1) { // If currently on photo upload step
      if (selectedFiles.value.length > 0) {
        analyzePhotos(); // Analyze photos if selected
      }
      activeStep.value = 2;
    } else if (currentStep === 2) { // If currently on general info step
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
  // Placeholder for photo analysis logic
  console.log('Analyzing photos:', selectedFiles.value);
  // In a real application, this would trigger an AI analysis service
  // and update internalMemory with analysis results (e.g., photoAnalysisId)
  // For now, it just logs.
  alert('Photo analysis triggered! (Check console for files)');
};

const fileToBase64 = (file: File): string => {
  if (!file) return '';
  // This is a synchronous placeholder. In a real app, use FileReader for async conversion.
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