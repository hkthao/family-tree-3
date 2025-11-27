<template>
  <v-row>
    <!-- Section 1: Uploaded Image -->
    <v-col cols="6">
      <v-img v-if="memoryStore.faceRecognition.uploadedImage" :src="memoryStore.faceRecognition.uploadedImage"
        max-height="200" contain class="mb-4"></v-img>

    </v-col>

    <v-col cols="6">
      <v-chip-group v-if="internalMemory.faces && internalMemory.faces.length > 0" column>
        <v-chip v-for="(face, index) in internalMemory.faces" :key="index" class="mb-2 me-2" size="large"
          :prepend-avatar="createBase64ImageSrc(face.thumbnail)">
          {{ face.memberName || t('common.unknown') }}
          <span v-if="face.relationPrompt" class="ml-1 text-medium-emphasis">({{ face.relationPrompt }})</span>
        </v-chip>
      </v-chip-group>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <v-chip-group column>
        <!-- Section 3: User Selections (Event, Context, Emotion, Perspective) -->
        <!-- Event Suggestion -->
        <div v-if="internalMemory.eventSuggestion">
          <v-chip size="large">
            {{ internalMemory.eventSuggestion }}
            <span v-if="internalMemory.customEventDescription && internalMemory.eventSuggestion === 'unsure'">
              ({{ internalMemory.customEventDescription }})
            </span>
          </v-chip>
        </div>

        <!-- Emotion Context Tags -->
        <div v-if="internalMemory.emotionContextTags && internalMemory.emotionContextTags.length > 0">
          <v-chip-group column>
            <v-chip size="large" v-for="(tag, i) in internalMemory.emotionContextTags" :key="i">
              {{ tag }}
            </v-chip>
          </v-chip-group>
        </div>
        <div v-if="internalMemory.customEmotionContext">
          <v-chip size="large">{{ internalMemory.customEmotionContext }}</v-chip>
        </div>

        <!-- Perspective -->
        <v-chip size="large" v-if="internalMemory.perspective">
          {{aiPerspectiveSuggestions.find(p => p.value === internalMemory.perspective)?.text ||
            internalMemory.perspective}}
        </v-chip>

        <!-- Section 4: Raw Input & Style -->
        <v-chip v-if="internalMemory.rawInput" size="large">
          {{ internalMemory.rawInput }}
        </v-chip>

        <v-chip size="large">
          {{ internalMemory.storyStyle || t('common.na') }}
        </v-chip>
      </v-chip-group>
    </v-col>

    <!-- Generate Story Button -->
    <v-col cols="12" class="text-center">
      <v-btn color="primary" size="large" @click="generateStory" :loading="generatingStory"
        :disabled="!canGenerateStory || generatingStory" class="mb-4">
        {{ t('memory.create.step4.generateStory') }}
      </v-btn>
    </v-col>

    <!-- Section 5: Editable Story Title and Content -->
    <v-col cols="12" v-if="generatedStory">
      <h4 class="mb-2">{{ t('memory.create.step4.reviewEdit') }}</h4>
      <v-text-field v-model="internalMemory.title" :label="t('memory.storyEditor.title')" outlined
        class="mb-4"></v-text-field>
      <v-textarea v-model="internalMemory.story" :label="t('memory.storyEditor.storyContent')" outlined rows="10"
        auto-grow></v-textarea>
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