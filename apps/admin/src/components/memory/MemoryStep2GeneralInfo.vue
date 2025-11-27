<template>
  <v-form ref="formStep2">
    <v-row>
      <v-col cols="12">
        <h4>{{ t('memory.create.aiCharacterSuggestion.title') }}</h4>
        <v-row v-if="internalMemory.faces && internalMemory.faces.length > 0">
          <v-col v-for="face in internalMemory.faces" :key="face.id" cols="6">
            <v-card>
              <v-img class="rounded-sm my-4" :src="createBase64ImageSrc(face.thumbnail)" height="100px" contain></v-img>
              <MemberAutocomplete v-model="face.memberId" :label="t('member.form.member')" >
              </MemberAutocomplete>
              <v-text-field class="mt-2" v-model="face.relationPrompt"
                :label="t('memory.create.aiCharacterSuggestion.relationPrompt')" :readonly="readonly"></v-text-field>
            </v-card>
          </v-col>
        </v-row>
        <p v-else class="text-body-2 text-grey">{{ t('memory.create.aiCharacterSuggestion.noFacesDetected') }}</p>
      </v-col>

      <v-col cols="12">
           <v-alert
          type="info"
          variant="tonal"
          class="mb-4"
          icon="mdi-robot-outline"
        >
          {{ t('memory.create.aiSuggestionDisclaimer') }}
            <template v-slot:append>
                <v-btn
                  v-if="!readonly && internalMemory.faces && internalMemory.faces.length > 0"
                  color="primary"
                  prepend-icon="mdi-lightbulb-on-outline"
                  variant="text"
                  size="small"
                  @click="triggerAiAnalysis"
                  :loading="isAnalyzingAi"
                  :disabled="isAnalyzingAi"
                >
                  {{ t('memory.create.analyzePhotoWithAi') }}
                </v-btn>
            </template>
        </v-alert>

        <h4>{{ t('memory.create.aiEventSuggestion.title') }}</h4>
        <v-chip-group :model-value="internalMemory.eventSuggestion" @update:model-value="(newValue) => emit('update:modelValue', { ...props.modelValue, eventSuggestion: newValue })" color="primary" mandatory column :disabled="readonly">
          <v-chip v-for="eventItem in aiEventSuggestions" :key="eventItem.text" :value="eventItem.text" filter variant="tonal">
            <v-icon v-if="eventItem.isAi" size="small" start>mdi-robot-outline</v-icon>
            {{ eventItem.text }}
          </v-chip>
          <v-chip value="unsure" filter variant="tonal">
            {{ t('memory.create.aiEventSuggestion.unsure') }}
          </v-chip>
        </v-chip-group>
        <v-text-field class="my-2" v-if="internalMemory.eventSuggestion === 'unsure'"
          v-model="internalMemory.customEventDescription"
          :label="t('memory.create.aiEventSuggestion.customDescription')" clearable :readonly="readonly"></v-text-field>
        <h4>{{ t('memory.create.aiEmotionContextSuggestion.title') }}</h4>
        <v-chip-group :model-value="internalMemory.emotionContextTags" @update:model-value="(newValue) => emit('update:modelValue', { ...props.modelValue, emotionContextTags: newValue })" multiple filter color="primary" column :disabled="readonly">
          <v-chip v-for="emotionItem in aiEmotionContextSuggestions" :key="emotionItem.text" :value="emotionItem.text" filter variant="tonal">
            <v-icon v-if="emotionItem.isAi" size="small" start>mdi-robot-outline</v-icon>
            {{ emotionItem.text }}
          </v-chip>
        </v-chip-group>
      </v-col>

      <v-col cols="12">
        <v-textarea
          v-model="internalMemory.rawInput"
          :label="t('memory.create.rawInputPlaceholder')"
          :readonly="readonly"
          outlined
          rows="5"
          auto-grow
        ></v-textarea>
        <StoryStyleInput v-model="internalMemory.storyStyle" :readonly="readonly" />
      </v-col>
      <!-- END NEW -->

      <v-col cols="12">
        <h4>{{ t('memory.create.perspective.question') }}</h4>
        <v-chip-group :model-value="internalMemory.perspective" @update:model-value="(newValue) => emit('update:modelValue', { ...props.modelValue, perspective: newValue })" color="primary" mandatory column :disabled="readonly">
          <v-chip :value="aiPerspectiveSuggestions[0].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[0].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[1].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[1].text }}
          </v-chip>
          <v-chip :value="aiPerspectiveSuggestions[2].value" filter variant="tonal">
            {{ aiPerspectiveSuggestions[2].text }}
          </v-chip>
        </v-chip-group>
      </v-col>

    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, computed, type ComputedRef, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberAutocomplete } from '@/components/common';
import type { MemoryDto } from '@/types/memory';
import type { AiPhotoAnalysisInputDto } from '@/types/ai'; // NEW IMPORT
import type { MemoryFaceState } from '@/stores/memory.store'; // NEW IMPORT
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // NEW IMPORT
import { useMemoryStore } from '@/stores/memory.store'; // NEW IMPORT
import { createBase64ImageSrc } from '@/utils/image.utils'; // NEW IMPORT

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null;
}>();

const emit = defineEmits(['update:modelValue', 'aiAnalysisCompleted']); // NEW EMIT

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar(); // NEW
const memoryStore = useMemoryStore(); // NEW
const formStep2 = ref<HTMLFormElement | null>(null);
const isAnalyzingAi = ref(false); // NEW loading state

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

const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      rawInput: model.rawInput ?? undefined,
      story: model.story ?? undefined,
      eventSuggestion: model.eventSuggestion ?? undefined, // No longer reference aiEventSuggestions directly
      customEventDescription: model.customEventDescription ?? undefined,
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined,
      faces: model.faces ?? [],
      perspective: model.perspective ?? aiPerspectiveSuggestions.value[0].value,
    } as MemoryDto;
  },
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

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
    targetFaceCropUrl: undefined, // Initialize as undefined

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

  if (targetMemberFace) {
    aiInput.targetFaceCropUrl = targetMemberFace.thumbnailUrl || targetMemberFace.thumbnail; // Assign the thumbnail URL if target face exists

    if (targetMemberFace.memberId) {
              aiInput.memberInfo = {
                id: targetMemberFace.memberId,
                name: targetMemberFace.memberName,
                age: targetMemberFace.birthYear
                  ? (targetMemberFace.deathYear
                      ? targetMemberFace.deathYear - targetMemberFace.birthYear
                      : new Date().getFullYear() - targetMemberFace.birthYear)
                  : undefined,
              };    }
  }

  aiInput.otherFacesSummary = memory.faces!
    .filter(face => face.id !== aiInput.targetFaceId)
    .map(face => ({
      emotionLocal: face.emotion || '',
    }));

  return aiInput;
};

watch(() => props.memberId, (newMemberId) => {
  if (newMemberId && !internalMemory.value.memberId) {
    internalMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

const triggerAiAnalysis = async () => {
  if (props.readonly) return;

  isAnalyzingAi.value = true;
  try {
    const aiInput = buildAiPhotoAnalysisInput(internalMemory.value, memoryStore.faceRecognition);
    const aiResult = await memoryStore.services.ai.analyzePhoto({ Input: aiInput });

    if (aiResult.ok) {
      internalMemory.value.photoAnalysisResult = aiResult.value;
      showSnackbar(t('memory.create.aiAnalysisSuccess'), 'success');

      // Append AI generated event suggestions
      if (aiResult.value.suggestions?.event && aiResult.value.suggestions.event.length > 0) {
        const newAiEvents: SuggestionItem[] = aiResult.value.suggestions.event.map(text => ({ text, isAi: true }));
        // Only add unique AI suggestions
        newAiEvents.forEach(newItem => {
          if (!defaultEventSuggestions.value.some((existingItem: SuggestionItem) => existingItem.text === newItem.text)) {
            defaultEventSuggestions.value.push(newItem);
          }
        });
      }

      // Append AI generated emotion suggestions
      if (aiResult.value.suggestions?.emotion && aiResult.value.suggestions.emotion.length > 0) {
        const newAiEmotions: SuggestionItem[] = aiResult.value.suggestions.emotion.map(text => ({ text, isAi: true }));
        // Only add unique AI suggestions
        newAiEmotions.forEach(newItem => {
          if (!defaultEmotionContextSuggestions.value.some((existingItem: SuggestionItem) => existingItem.text === newItem.text)) {
            defaultEmotionContextSuggestions.value.push(newItem);
          }
        });
      }
      // Removed emit('aiAnalysisCompleted') to prevent automatic step advancement
    } else {
      showSnackbar(
        aiResult.error?.message || t('memory.errors.aiAnalysisFailed'),
        'error'
      );
    }
  } catch (error: any) {
    showSnackbar(error.message || t('memory.errors.aiAnalysisFailed'), 'error');
  } finally {
    isAnalyzingAi.value = false;
  }
};

// AI Event Suggestions (combines default and AI-generated, ensures uniqueness)
const aiEventSuggestions: ComputedRef<SuggestionItem[]> = computed(() => {
  const suggestions = [...defaultEventSuggestions.value];
  if (internalMemory.value.photoAnalysisResult?.suggestions?.event) {
    internalMemory.value.photoAnalysisResult.suggestions.event.forEach(aiText => {
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
  if (internalMemory.value.photoAnalysisResult?.suggestions?.emotion) {
    internalMemory.value.photoAnalysisResult.suggestions.emotion.forEach(aiText => {
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

// NEW: Import StoryStyleInput
import StoryStyleInput from './StoryStyleInput.vue';

const validate = async () => {
  return formStep2.value ? (await formStep2.value.validate()).valid : false;
};

defineExpose({
  validate,
  triggerAiAnalysis,
});
</script>