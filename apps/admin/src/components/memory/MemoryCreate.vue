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
              <v-card-text v-if="generatedStory">
                <StoryEditor :draft="generatedStory" @update:draft="onUpdateGeneratedStory" />
              </v-card-text>
              <v-card-text v-else>
                <v-alert type="info">{{ t('memory.create.step4.noStoryGenerated') }}</v-alert>
              </v-card-text>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" @click="saveMemory" :disabled="!generatedStory || savingMemory" :loading="savingMemory">
                  {{ t('memory.create.step5.save') }}
                </v-btn>
                <v-btn color="secondary" @click="exportPdf" :disabled="!generatedStory">{{ t('memory.create.step5.exportPdf') }}</v-btn>
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
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import PhotoAnalyzerPreview from './PhotoAnalyzerPreview.vue'; 
import StoryEditor from './StoryEditor.vue'; 
import router from '@/router';

interface Props {
  memberId: string;
}
const props = defineProps<Props>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const currentStep = ref(1);

// Step 1: Choose Photo
const photoFile = ref<File | null>(null);
const photoPreviewUrl = ref<string | null>(null);
const analyzingPhoto = ref(false);

// Step 2: Photo Analysis
const photoAnalysisResult = ref<any | null>(null);
const photoAnalysisId = ref<string | null>(null);

// Step 3: Raw Text Input
const rawText = ref('');
const storyStyle = ref('nostalgic'); // Default style
const storyStyles = ['nostalgic', 'warm', 'formal', 'folk'];
const memoryTitle = ref('');
const memoryYear = ref<number | null>(null);
const memoryTags = ref<string[]>([]);
const generatingStory = ref(false);

// Step 4: Story Editor
const generatedStory = ref<any | null>(null);
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

const analyzePhoto = async () => {
  if (!photoFile.value) return;

  analyzingPhoto.value = true;
  const formData = new FormData();
  formData.append('file', photoFile.value);
  formData.append('memberId', props.memberId);

  const result = await memoryStore.analyzePhoto(formData);
  if (result.ok) {
    photoAnalysisResult.value = result.value;
    photoAnalysisId.value = result.value?.id || null; // Fix: Handle potential undefined/null id
    currentStep.value = 2; // Move to Photo Analysis step
  } else {
    // snackbarStore.showSnackbar('Error analyzing photo: ' + result.error, 'error'); // Removed
  }
  analyzingPhoto.value = false;
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
  if (photoAnalysisResult.value?.summary) {
    rawText.value = photoAnalysisResult.value.summary;
  }
  // Optionally use other analysis results for tags, year, etc.
  if (photoAnalysisResult.value?.yearEstimate) {
    memoryYear.value = parseInt(photoAnalysisResult.value.yearEstimate);
  }
  // Add tags from objects or events
  if (photoAnalysisResult.value?.objects && photoAnalysisResult.value.objects.length > 0) {
    memoryTags.value = [...new Set([...memoryTags.value, ...photoAnalysisResult.value.objects])];
  }
  if (photoAnalysisResult.value?.event) {
    memoryTags.value = [...new Set([...memoryTags.value, photoAnalysisResult.value.event])];
  }

  currentStep.value = 3; // Move to Raw Text Input
};

const editPhotoContext = () => {
  // Allow user to edit the photoAnalysisResult before proceeding
  // For now, this just means they can go back and re-analyze or skip.
  // snackbarStore.showSnackbar('Editing photo context is not yet implemented. Please adjust raw text manually.', 'info'); // Removed
  currentStep.value = 3;
};

const canGenerateStory = computed(() => {
  return (rawText.value && rawText.value.length >= 10) || photoAnalysisId.value;
});

const generateStory = async () => {
  if (!canGenerateStory.value) return;

  generatingStory.value = true;
  const requestPayload = {
    memberId: props.memberId,
    photoAnalysisId: photoAnalysisId.value,
    rawText: rawText.value,
    style: storyStyle.value,
    maxWords: 500, // Hardcoded for now
  };

  const result = await memoryStore.generateStory(requestPayload);
  if (result.ok) {
    generatedStory.value = result.value;
    if (!memoryTitle.value) memoryTitle.value = generatedStory.value.title;
    if (generatedStory.value.tags && generatedStory.value.tags.length > 0) {
        memoryTags.value = [...new Set([...memoryTags.value, ...generatedStory.value.tags])];
    }
    currentStep.value = 4; // Move to Story Editor
  } else {
    // snackbarStore.showSnackbar('Error generating story: ' + result.error, 'error'); // Removed
  }
  generatingStory.value = false;
};

const onUpdateGeneratedStory = (draft: any) => {
  generatedStory.value = draft; // Update local state when StoryEditor emits changes
};

const saveMemory = async () => {
  if (!generatedStory.value) return;

  savingMemory.value = true;
  const createPayload = {
    memberId: props.memberId,
    title: memoryTitle.value || generatedStory.value.title,
    story: generatedStory.value.draftStory,
    photoAnalysisId: photoAnalysisId.value,
    photoUrl: photoPreviewUrl.value, // Save the original photo URL (or restored if we integrate photo revival)
    tags: memoryTags.value,
    keywords: generatedStory.value.keywords || [],
  };

  const result = await memoryStore.addItem(createPayload);
  if (result.ok) {
    savedMemoryId.value = result.value.id || null; // Access id from result.value
    // snackbarStore.showSnackbar(t('memory.create.step5.saveSuccess'), 'success'); // Removed
    emit('saved', savedMemoryId.value);
    currentStep.value = 5; // Move to Review & Save
  }
  else {
    // snackbarStore.showSnackbar('Error saving memory: ' + result.error, 'error'); // Removed
  }
  savingMemory.value = false;
};

const exportPdf = () => {
  // snackbarStore.showSnackbar('Export to PDF is not yet implemented.', 'info'); // Removed
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

</script>

<style scoped>
/* Scoped styles for MemoryCreate */
</style>
