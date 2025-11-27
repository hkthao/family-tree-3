<template>
  <v-row>
    <!-- Section 1: Uploaded Image -->
    <v-col cols="12" v-if="memoryStore.faceRecognition.uploadedImage">
      <h4 class="mb-2">{{ t('memory.create.step1.title') }}</h4>
      <v-img :src="memoryStore.faceRecognition.uploadedImage" max-height="200" contain class="mb-4"></v-img>
    </v-col>

    <!-- Section 2: Detected Faces & Relationships -->
    <v-col cols="12" v-if="internalMemory.faces && internalMemory.faces.length > 0">
      <h4 class="mb-2">{{ t('memory.create.aiCharacterSuggestion.title') }}</h4>
      <v-list density="compact" class="mb-4">
        <v-list-item v-for="(face, index) in internalMemory.faces" :key="index">
          <template v-slot:prepend>
            <v-avatar size="40" rounded="sm" class="mr-2">
              <v-img :src="createBase64ImageSrc(face.thumbnail)" alt="Face"></v-img>
            </v-avatar>
          </template>
          <v-list-item-title>
            {{ t('member.form.member') }}: {{ face.memberName || t('common.unknown') }}
          </v-list-item-title>
          <v-list-item-subtitle v-if="face.relationPrompt">
            {{ t('memory.create.aiCharacterSuggestion.relationPrompt') }}: {{ face.relationPrompt }}
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>
    </v-col>

    <!-- Section 3: User Selections (Event, Context, Emotion, Perspective) -->
    <v-col cols="12">
      <h4 class="mb-2">{{ t('memory.create.step2.title') }}</h4>

      <p class="mb-1" v-if="internalMemory.eventSuggestion">
        <strong>{{ t('memory.create.aiEventSuggestion.title') }}:</strong> {{ internalMemory.eventSuggestion }}
        <span v-if="internalMemory.customEventDescription && internalMemory.eventSuggestion === 'unsure'">
          ({{ internalMemory.customEventDescription }})
        </span>
      </p>

      <p class="mb-1" v-if="internalMemory.emotionContextTags && internalMemory.emotionContextTags.length > 0">
        <strong>{{ t('memory.create.aiEmotionContextSuggestion.title') }}:</strong> {{
          internalMemory.emotionContextTags.join(', ') }}
      </p>
      <p class="mb-1" v-if="internalMemory.customEmotionContext">
        <strong>{{ t('memory.create.aiEmotionContextSuggestion.customInput') }}:</strong> {{
          internalMemory.customEmotionContext }}
      </p>
      <p class="mb-4" v-if="internalMemory.perspective">
        <strong>{{ t('memory.create.perspective.question') }}:</strong>
        {{ aiPerspectiveSuggestions.find(p => p.value === internalMemory.perspective)?.text || internalMemory.perspective }}
      </p>
    </v-col>

    <!-- Section 4: Raw Input & Style -->
    <v-col cols="12" class="mb-4">
      <h4 class="mb-2">{{ t('memory.create.step3.rawTextInput') }}</h4>
      <p class="mb-1"><strong>{{ t('memory.create.rawInputPlaceholder') }}:</strong> {{ internalMemory.rawInput || t('common.na') }}</p>
      <p class="mb-1"><strong>{{ t('memory.create.storyStyle.question') }}:</strong> {{ internalMemory.storyStyle || t('common.na') }}</p>
    </v-col>

    <!-- Generate Story Button -->
    <v-col cols="12" class="text-center">
      <v-btn
        color="primary"
        size="large"
        @click="generateStory"
        :loading="generatingStory"
        :disabled="!canGenerateStory || generatingStory"
        class="mb-4"
      >
        {{ t('memory.create.step4.generateStory') }}
      </v-btn>
    </v-col>

    <!-- Section 5: Editable Story Title and Content -->
    <v-col cols="12" v-if="generatedStory">
      <h4 class="mb-2">{{ t('memory.create.step4.reviewEdit') }}</h4>
      <v-text-field
        v-model="internalMemory.title"
        :label="t('memory.storyEditor.title')"
        outlined
        class="mb-4"
      ></v-text-field>
      <v-textarea
        v-model="internalMemory.story"
        :label="t('memory.storyEditor.storyContent')"
        outlined
        rows="10"
        auto-grow
      ></v-textarea>
    </v-col>
    <v-col cols="12" v-else>
      <v-alert type="info" variant="tonal">{{ t('memory.create.step4.noStoryGenerated') }}</v-alert>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory';
import { useMemoryStore } from '@/stores/memory.store'; // Use the main memory store
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { PhotoAnalysisPersonDto } from '@/types/ai'; // NEW IMPORT
import { createBase64ImageSrc } from '@/utils/image.utils'; // NEW IMPORT

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'story-generated']); // Added new emit event

const { t } = useI18n();
const memoryStore = useMemoryStore(); // Initialize the main memory store
const { showSnackbar } = useGlobalSnackbar();

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => { emit('update:modelValue', value); }, // Allow setting for v-model
});

const generatedStory = ref<string | null>(null);
const generatedTitle = ref<string | null>(null);
const generatingStory = ref(false);
const storyEditorValid = ref(true); // To track validation from StoryEditor

const canGenerateStory = computed(() => {
  // Simplified condition for this step; refine based on actual requirements
  return (internalMemory.value.rawInput && internalMemory.value.rawInput.length >= 10) ||
         (memoryStore.faceRecognition.uploadedImageId && memoryStore.faceRecognition.detectedFaces.length > 0);
});

const generateStory = async () => {
  if (!canGenerateStory.value) return;

  generatingStory.value = true;

  const photoPersonsPayload: PhotoAnalysisPersonDto[] = memoryStore.faceRecognition.detectedFaces.map(face => ({
    id: face.id,
    memberId: face.memberId,
    name: face.memberName,
    emotion: face.emotion,
    confidence: face.emotionConfidence, // Assuming confidence is what emotionConfidence maps to
    relationPrompt: face.relationPrompt, // Include relationPrompt
  }));

  const requestPayload = {
    memberId: internalMemory.value.memberId,
    photoAnalysisId: memoryStore.faceRecognition.uploadedImageId,
    rawText: internalMemory.value.rawInput,
    style: internalMemory.value.storyStyle,
    maxWords: 500, // Hardcoded for now

    // New granular photo analysis properties
    photoSummary: internalMemory.value.photoAnalysisResult?.summary,
    photoScene: internalMemory.value.photoAnalysisResult?.scene,
    photoEventAnalysis: internalMemory.value.photoAnalysisResult?.event,
    photoEmotionAnalysis: internalMemory.value.photoAnalysisResult?.emotion,
    photoYearEstimate: internalMemory.value.photoAnalysisResult?.yearEstimate,
    photoObjects: internalMemory.value.photoAnalysisResult?.objects,
    photoPersons: photoPersonsPayload,

    // User selected context
    perspective: internalMemory.value.perspective,
    event: internalMemory.value.eventSuggestion,
    customEventDescription: internalMemory.value.customEventDescription,
    emotionContexts: internalMemory.value.emotionContextTags,
  };

  const result = await memoryStore.generateStory(requestPayload);
  if (result.ok) {
    generatedStory.value = result.value.draftStory;
    generatedTitle.value = result.value.title;
    // Update internalMemory directly with the generated story and title
    internalMemory.value.story = generatedStory.value;
    internalMemory.value.title = generatedTitle.value;
    emit('story-generated', { story: generatedStory.value, title: generatedTitle.value });
  } else {
    showSnackbar(result.error?.message || t('memory.errors.storyGenerationFailed'), 'error');
  }
  generatingStory.value = false;
};

// Expose properties for parent access
defineExpose({
  generateStory,
  generatedStory,
  generatedTitle,
  storyEditorValid,
});

// AI Perspective Suggestions (remains fixed as no AI suggestions for this)
const aiPerspectiveSuggestions = ref([
  { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
  { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
  { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
]);
</script>