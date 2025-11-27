<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <v-btn icon @click="emit('close')" variant="text">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      <span class="text-h6">{{ t('memory.create.title') }}</span>
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-stepper v-model="currentStep" flat>
        <v-stepper-header>
          <v-stepper-item
            :title="t('memory.create.step1.title')"
            :value="1"
            :complete="currentStep > 1"
            editable
          ></v-stepper-item>
          <v-divider></v-divider>
          <v-stepper-item
            :title="t('memory.create.step2.title')"
            :value="2"
            :complete="currentStep > 2"
            editable
            v-if="photoFile"
          ></v-stepper-item>
          <v-divider v-if="photoFile"></v-divider>
          <v-stepper-item
            :title="t('memory.create.step3.title')"
            :value="3"
            :complete="currentStep > 3"
            editable
          ></v-stepper-item>
          <v-divider></v-divider>
          <v-stepper-item
            :title="t('memory.create.step4.title')"
            :value="4"
            :complete="currentStep > 4"
            editable
          ></v-stepper-item>
          <v-divider></v-divider>
          <v-stepper-item
            :title="t('memory.create.step5.title')"
            :value="5"
            :complete="currentStep > 5"
            editable
          ></v-stepper-item>
        </v-stepper-header>

        <v-stepper-window v-model="currentStep">
          <!-- Step 1: Choose Photo -->
          <v-stepper-window-item :value="1">
            <v-card class="mb-5" flat>
              <v-card-title>{{ t('memory.create.step1.choosePhoto') }}</v-card-title>
              <v-card-text>
                <v-file-input
                  v-model="photoFile"
                  :label="t('memory.create.step1.uploadPhoto')"
                  accept="image/jpeg,image/png,image/heic"
                  prepend-icon="mdi-camera"
                  show-size
                  counter
                  @change="onFileChange"
                ></v-file-input>
                <div v-if="photoPreviewUrl" class="my-3 text-center">
                  <v-img :src="photoPreviewUrl" max-height="200" contain></v-img>
                </div>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn
                  color="primary"
                  :disabled="!photoFile || analyzingPhoto"
                  @click="analyzePhoto"
                  :loading="analyzingPhoto"
                >
                  {{ t('memory.create.step1.analyzePhoto') }}
                </v-btn>
                <v-btn color="secondary" @click="skipPhotoAnalysis">
                  {{ t('memory.create.step1.skipToText') }}
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-stepper-window-item>

          <!-- Step 2: Photo Analysis Preview (Conditional) -->
          <v-stepper-window-item :value="2" v-if="photoFile">
            <v-card class="mb-5" flat>
              <v-card-title>{{ t('memory.create.step2.analysisResult') }}</v-card-title>
              <v-card-text v-if="photoAnalysisResult">
                <PhotoAnalyzerPreview :analysis-result="photoAnalysisResult" @use-context="usePhotoContext" @edit-context="editPhotoContext" @skip="skipPhotoAnalysis" />
              </v-card-text>
              <v-card-text v-else>
                <v-alert type="info">{{ t('memory.create.step2.noAnalysisYet') }}</v-alert>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" @click="usePhotoContext">{{ t('memory.create.step2.useContext') }}</v-btn>
                <v-btn color="secondary" @click="skipPhotoAnalysis">{{ t('memory.create.step2.skip') }}</v-btn>
              </v-card-actions>
            </v-card>
          </v-stepper-window-item>

          <!-- Step 3: Raw Text Input -->
          <v-stepper-window-item :value="3">
            <v-card class="mb-5" flat>
              <v-card-title>{{ t('memory.create.step3.rawTextInput') }}</v-card-title>
              <v-card-text>
                <v-textarea
                  v-model="rawText"
                  :label="t('memory.create.step3.placeholder')"
                  outlined
                  rows="5"
                  auto-grow
                ></v-textarea>
                <v-select
                  v-model="storyStyle"
                  :items="storyStyles"
                  :label="t('memory.create.step3.storyStyle')"
                  outlined
                ></v-select>
                <v-text-field v-model="memoryTitle" :label="t('memory.create.step3.titleOptional')"></v-text-field>
                <v-text-field v-model="memoryYear" :label="t('memory.create.step3.yearOptional')" type="number"></v-text-field>
                <v-combobox
                  v-model="memoryTags"
                  :label="t('memory.create.step3.tagsOptional')"
                  multiple
                  chips
                ></v-combobox>

                <v-select
                  v-model="perspectiveSelection"
                  :items="aiPerspectiveSuggestions"
                  item-title="text"
                  item-value="value"
                  :label="t('memory.create.perspective.title')"
                  outlined
                  class="mt-4"
                ></v-select>

                <h4 class="mt-4">{{ t('memory.create.aiEventSuggestion.title') }}</h4>
                <v-chip-group
                  v-model="eventSelection"
                  color="primary"
                  mandatory
                  column
                >
                  <v-chip v-for="eventItem in aiEventSuggestions" :key="eventItem.text" :value="eventItem.text" filter variant="tonal">
                    <v-icon v-if="eventItem.isAi" size="small" start>mdi-robot-outline</v-icon>
                    {{ eventItem.text }}
                  </v-chip>
                  <v-chip value="unsure" filter variant="tonal">
                    {{ t('memory.create.aiEventSuggestion.unsure') }}
                  </v-chip>
                </v-chip-group>
                <v-text-field
                  v-if="eventSelection === 'unsure'"
                  v-model="customEventDescriptionInput"
                  :label="t('memory.create.aiEventSuggestion.customDescription')"
                  clearable
                  class="my-2"
                ></v-text-field>

                <h4 class="mt-4">{{ t('memory.create.aiEmotionContextSuggestion.title') }}</h4>
                <v-chip-group
                  v-model="emotionContextsSelection"
                  multiple
                  filter
                  color="primary"
                  column
                >
                  <v-chip v-for="emotionItem in aiEmotionContextSuggestions" :key="emotionItem.text" :value="emotionItem.text" filter variant="tonal">
                    <v-icon v-if="emotionItem.isAi" size="small" start>mdi-robot-outline</v-icon>
                    {{ emotionItem.text }}
                  </v-chip>
                </v-chip-group>

              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" @click="generateStory" :disabled="!canGenerateStory" :loading="generatingStory">
                  {{ t('memory.create.step4.generateStory') }}
                </v-btn>
              </v-card-actions>
            </v-card>
          </v-stepper-window-item>

          <!-- Step 4: Story Editor -->
          <v-stepper-window-item :value="4">
            <v-card class="mb-5" flat>
              <v-card-title>{{ t('memory.create.step4.reviewEdit') }}</v-card-title>
              <v-card-text v-if="story">
                <StoryEditor :draft="story" @update:draft="onUpdateGeneratedStory" />
              </v-card-text>
              <v-card-text v-else>
                <v-alert type="info">{{ t('memory.create.step4.noStoryGenerated') }}</v-alert>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" @click="saveMemory" :disabled="!story || savingMemory" :loading="savingMemory">
                  {{ t('memory.create.step5.save') }}
                </v-btn>
                <v-btn color="secondary" @click="exportPdf" :disabled="!story">{{ t('memory.create.step5.exportPdf') }}</v-btn>
              </v-card-actions>
            </v-card>
          </v-stepper-window-item>

          <!-- Step 5: Review & Save -->
          <v-stepper-window-item :value="5">
            <v-card class="mb-5" flat>
              <v-card-title>{{ t('memory.create.step5.reviewAndSave') }}</v-card-title>
              <v-card-text>
                <div v-if="savedMemoryId">
                  <v-alert type="success">{{ t('memory.create.step5.saveSuccess') }}</v-alert>
                  <v-btn color="primary" @click="viewSavedMemory">{{ t('memory.create.step5.viewMemory') }}</v-btn>
                </div>
                <div v-else>
                  <v-alert type="error">{{ t('memory.create.step5.saveFailed') }}</v-alert>
                </div>
              </v-card-text>
            </v-card>
          </v-stepper-window-item>
        </v-stepper-window>
      </v-stepper>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch, type Ref, type ComputedRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import PhotoAnalyzerPreview from './PhotoAnalyzerPreview.vue'; 
import StoryEditor from './StoryEditor.vue'; 
import router from '@/router';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // NEW IMPORT
import type { ExifDataDto } from '@/types/memory'; // NEW IMPORT
import type {
  AiPhotoAnalysisInputDto,
  PhotoAnalysisResultDto, // Keep PhotoAnalysisResultDto
  PhotoAnalysisPersonDto, // NEW IMPORT
} from '@/types/ai'; // Only import necessary AI DTOs


interface Props {
  memberId: string;
}
const props = defineProps<Props>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar(); // Initialize snackbar
const currentStep = ref(1);

// Step 1: Choose Photo
const photoFile = ref<File | null>(null);
const photoPreviewUrl = ref<string | null>(null);
const analyzingPhoto = ref(false);

// Step 2: Photo Analysis
const photoAnalysisResult = ref<PhotoAnalysisResultDto | null>(null); // Use PhotoAnalysisResultDto
const photoAnalysisId = ref<string | null>(null); // Will store createdAt from analysis result

// Step 3: Raw Text Input
const rawText = ref('');
const storyStyle = ref('nostalgic'); // Default style
const storyStyles = ['nostalgic', 'warm', 'formal', 'folk'];
const memoryTitle = ref('');
const memoryYear = ref<number | null>(null);
const memoryTags = ref<string[]>([]);
const generatingStory = ref(false);

// NEW: Local state for user selections from Step 2 equivalent inputs
const perspectiveSelection = ref<string | undefined>(undefined);
const eventSelection = ref<string | undefined>(undefined);
const customEventDescriptionInput = ref<string | undefined>(undefined);
const emotionContextsSelection = ref<string[]>([]);

// NEW: SuggestionItem interface and suggestion lists for local use
interface SuggestionItem {
  text: string;
  isAi: boolean;
}

const defaultEventSuggestions: Ref<SuggestionItem[]> = ref([
  { text: t('memory.create.aiEventSuggestion.suggestion1'), isAi: false },
  { text: t('memory.create.aiEventSuggestion.suggestion2'), isAi: false },
  { text: t('memory.create.aiEventSuggestion.suggestion3'), isAi: false },
  { text: t('memory.create.aiEventSuggestion.suggestion4'), isAi: false },
]);

const defaultEmotionContextSuggestions: Ref<SuggestionItem[]> = ref([
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion1'), isAi: false },
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion2'), isAi: false },
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion3'), isAi: false },
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion4'), isAi: false },
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion5'), isAi: false },
  { text: t('memory.create.aiEmotionContextSuggestion.suggestion6'), isAi: false },
]);

// AI Event Suggestions (combines default and AI-generated, ensures uniqueness)
const aiEventSuggestions: ComputedRef<SuggestionItem[]> = computed(() => {
  const suggestions = [...defaultEventSuggestions.value];
  if (photoAnalysisResult.value?.suggestions?.event) {
    photoAnalysisResult.value.suggestions.event.forEach(aiText => {
      // Add only if not already present in the combined list
      if (!suggestions.some(item => item.text === aiText)) {
        suggestions.push({ text: aiText, isAi: true });
      }
    });
  }
  return suggestions;
});

// AI Emotion & Context Suggestions (combines default and AI-generated, ensures uniqueness)
const aiEmotionContextSuggestions: ComputedRef<SuggestionItem[]> = computed(() => {
  const suggestions = [...defaultEmotionContextSuggestions.value];
  if (photoAnalysisResult.value?.suggestions?.emotion) {
    photoAnalysisResult.value.suggestions.emotion.forEach(aiText => {
      // Add only if not already present in the combined list
      if (!suggestions.some(item => item.text === aiText)) {
        suggestions.push({ text: aiText, isAi: true });
      }
    });
  }
  return suggestions;
});

// AI Perspective Suggestions (remains fixed as no AI suggestions for this)
const aiPerspectiveSuggestions = ref([
  { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
  { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
  { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
]);

// Step 4: Story Editor
const story = ref<any | null>(null);
const savingMemory = ref(false);

// Step 5: Review & Save
const savedMemoryId = ref<string | null>(null);

const onFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files[0]) {
    photoFile.value = target.files[0];
    photoPreviewUrl.value = URL.createObjectURL(photoFile.value);
  } else {
    photoFile.value = null;
    photoPreviewUrl.value = null;
  }
};

// Helper function to load image and get dimensions
const loadImage = (file: File): Promise<HTMLImageElement> => {
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    img.src = URL.createObjectURL(file);
  });
};

// Function to simulate EXIF extraction (placeholder)
const extractExifData = async (file: File): Promise<ExifDataDto | undefined> => {
  console.log('Attempting to extract EXIF from file:', file.name);
  return {
    datetime: new Date().toISOString(),
    gps: '50.123, 10.456',
    cameraInfo: 'DummyCameraModel',
  };
};

const analyzePhoto = async () => {
  if (!photoFile.value) return;

  analyzingPhoto.value = true;
  
  try {
    // 1. Perform client-side face detection
    await memoryStore.detectFaces(photoFile.value, true);

    // 2. Extract image size
    let imageSize: string | undefined;
    try {
      const img = await loadImage(photoFile.value);
      imageSize = `${img.width}x${img.height}`;
    } catch (e) {
      console.error('Failed to load image for dimensions:', e);
    }

    // 3. Extract EXIF data
    let exifData: ExifDataDto | undefined;
    try {
      exifData = await extractExifData(photoFile.value);
    } catch (e) {
      console.error('Failed to extract EXIF data:', e);
    }

    // 4. Construct AiPhotoAnalysisInputDto
    const detectedFaces = memoryStore.faceRecognition.detectedFaces;
    const aiInput: AiPhotoAnalysisInputDto = {
      imageBase64: memoryStore.faceRecognition.uploadedImage || undefined,
      imageSize: imageSize,
      exif: exifData,
      // Default to first detected face as target if available, otherwise undefined
      targetFaceId: detectedFaces.length > 0 ? detectedFaces[0].id : undefined,
      targetFaceCropUrl: null, // Placeholder

      faces: detectedFaces.map(face => ({
        faceId: face.id,
        bbox: [face.boundingBox.x, face.boundingBox.y, face.boundingBox.width, face.boundingBox.height],
        emotionLocal: {
          dominant: face.emotion || '',
          confidence: face.emotionConfidence || 0,
        },
        quality: face.quality || 'unknown',
      })),
      memberInfo: undefined, // Requires more complex lookup
      otherFacesSummary: detectedFaces
        .filter(face => face.id !== (detectedFaces.length > 0 ? detectedFaces[0].id : undefined))
        .map(face => ({
          emotionLocal: face.emotion || '',
        })),
    };

    // 5. Call AI analysis action
    const result = await memoryStore.services.ai.analyzePhoto({ Input: aiInput }); // WRAP aiInput IN { Input: ... }
    
    if (result.ok) {
      photoAnalysisResult.value = result.value;
      photoAnalysisId.value = result.value?.createdAt || null; // Use createdAt as ID
      currentStep.value = 2; // Move to Photo Analysis step
    } else {
      showSnackbar(result.error?.message || t('memory.errors.aiAnalysisFailed'), 'error');
    }
  } catch (error: any) {
    console.error('Error during photo analysis setup:', error);
    showSnackbar(error.message || t('memory.errors.unexpectedError'), 'error');
  } finally {
    analyzingPhoto.value = false;
  }
};

const skipPhotoAnalysis = () => {
  photoFile.value = null;
  photoPreviewUrl.value = null;
  photoAnalysisResult.value = null;
  photoAnalysisId.value = null;
  currentStep.value = 3; // Move directly to Raw Text Input
};

const usePhotoContext = () => {
  // Logic to use photo context to pre-fill rawText or suggest prompts
  if (photoAnalysisResult.value?.summary && !rawText.value) { // Only pre-fill if rawText is empty
    rawText.value = photoAnalysisResult.value.summary;
  }
  // Optionally use other analysis results for tags, year, etc.
  if (photoAnalysisResult.value?.yearEstimate && memoryYear.value === null) { // Only pre-fill if empty
    memoryYear.value = parseInt(photoAnalysisResult.value.yearEstimate);
  }
  // Add tags from objects or events (always append, not replace existing)
  if (photoAnalysisResult.value?.objects && photoAnalysisResult.value.objects.length > 0) {
    memoryTags.value = [...new Set([...memoryTags.value, ...photoAnalysisResult.value.objects])];
  }
  if (photoAnalysisResult.value?.event && !memoryTags.value.includes(photoAnalysisResult.value.event)) { // Only add if not already present
    memoryTags.value = [...new Set([...memoryTags.value, photoAnalysisResult.value.event])];
  }

  // NEW: Populate event and emotion selections from AI analysis results
  if (photoAnalysisResult.value?.event && eventSelection.value === undefined) { // Only pre-fill if empty
    eventSelection.value = photoAnalysisResult.value.event;
  }
  if (photoAnalysisResult.value?.emotion) {
    const newEmotionContexts = Array.isArray(photoAnalysisResult.value.emotion) ? photoAnalysisResult.value.emotion : [photoAnalysisResult.value.emotion];
    // Append only unique new emotion contexts
    newEmotionContexts.forEach(newEmotion => {
      if (!emotionContextsSelection.value.includes(newEmotion)) {
        emotionContextsSelection.value.push(newEmotion);
      }
    });
  }


  currentStep.value = 3; // Move to Raw Text Input
};

const editPhotoContext = () => {
  // Allow user to edit the photoAnalysisResult before proceeding
  // For now, this just means they can go back and re-analyze or skip.
  showSnackbar(t('memory.create.editContextNotImplemented'), 'info'); // Use snackbar for info
  currentStep.value = 3;
};

const canGenerateStory = computed(() => {
  return (rawText.value && rawText.value.length >= 10) || photoAnalysisId.value;
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
    memberId: props.memberId,
    photoAnalysisId: photoAnalysisId.value,
    rawText: rawText.value,
    style: storyStyle.value,
    maxWords: 500, // Hardcoded for now

    // New granular photo analysis properties
    photoSummary: photoAnalysisResult.value?.summary,
    photoScene: photoAnalysisResult.value?.scene,
    photoEventAnalysis: photoAnalysisResult.value?.event,
    photoEmotionAnalysis: photoAnalysisResult.value?.emotion,
    photoYearEstimate: photoAnalysisResult.value?.yearEstimate,
    photoObjects: photoAnalysisResult.value?.objects,
    photoPersons: photoPersonsPayload,

    // User selected context
    perspective: perspectiveSelection.value,
    event: eventSelection.value,
    customEventDescription: customEventDescriptionInput.value,
    emotionContexts: emotionContextsSelection.value,
  };

  const result = await memoryStore.generateStory(requestPayload);
  if (result.ok) {
    story.value = result.value;
    if (!memoryTitle.value) memoryTitle.value = story.value.title;
    if (story.value.tags && story.value.tags.length > 0) {
        memoryTags.value = [...new Set([...memoryTags.value, ...story.value.tags])];
    }
    currentStep.value = 4; // Move to Story Editor
  } else {
    showSnackbar(result.error?.message || t('memory.errors.storyGenerationFailed'), 'error'); // Use snackbar
  }
  generatingStory.value = false;
};

const onUpdateGeneratedStory = (draft: any) => {
  story.value = draft; // Update local state when StoryEditor emits changes
};

const saveMemory = async () => {
  if (!story.value) return;

  savingMemory.value = true;
  const createPayload = {
    memberId: props.memberId,
    title: memoryTitle.value || story.value.title,
    story: story.value.draftStory,
    photoAnalysisId: photoAnalysisId.value,
    photoUrl: photoPreviewUrl.value, // Save the original photo URL (or restored if we integrate photo revival)
    tags: memoryTags.value,
    keywords: story.value.keywords || [],
  };

  const result = await memoryStore.addItem(createPayload);
  if (result.ok) {
    savedMemoryId.value = result.value.id || null; // Access id from result.value
    showSnackbar(t('memory.create.step5.saveSuccess'), 'success'); // Use snackbar
    emit('saved', savedMemoryId.value);
    currentStep.value = 5; // Move to Review & Save
  }
  else {
    showSnackbar(result.error?.message || t('memory.errors.memorySaveFailed'), 'error'); // Use snackbar
  }
  savingMemory.value = false;
};

const exportPdf = () => {
  showSnackbar(t('memory.create.exportPdfNotImplemented'), 'info'); // Use snackbar
};

const viewSavedMemory = () => {
  if (savedMemoryId.value) {
    router.push({ name: 'MemoryDetail', params: { memoryId: savedMemoryId.value } });
    emit('close');
  }
};

watch(photoFile, (newFile) => {
  if (!newFile) {
    photoPreviewUrl.value = null;
    photoAnalysisResult.value = null;
    photoAnalysisId.value = null;
  }
});

watch(currentStep, (newStep) => {
  if (newStep === 3 && rawText.value === '' && photoAnalysisResult.value?.summary) {
    rawText.value = photoAnalysisResult.value.summary;
  }
});

</script>

<style scoped>
/* Scoped styles for MemoryCreate */
</style>
