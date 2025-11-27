<template>
  <v-form ref="formStep2">
    <v-row>
      <v-col cols="12">
        <h4>{{ t('memory.create.aiCharacterSuggestion.title') }}</h4>
        <v-row v-if="internalMemory.faces && internalMemory.faces.length > 0">
          <v-col v-for="face in internalMemory.faces" :key="face.id" cols="6">
            <v-card>
              <v-img class="rounded-sm my-4" :src="createBase64ImageSrc(face.thumbnail)" height="100px" contain></v-img>
              <MemberAutocomplete v-model="face.memberId" :label="t('member.form.member')" :disabled="true">
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
          <v-chip v-for="event in aiEventSuggestions" :key="event" :value="event" filter variant="tonal">
            {{ event }}
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
          <v-chip v-for="tag in aiEmotionContextSuggestions" :key="tag" :value="tag" filter variant="tonal">
            {{ tag }}
          </v-chip>
        </v-chip-group>
      </v-col>


      <v-col cols="12">
        <v-textarea v-model="internalMemory.rawInput" :label="t('memory.create.rawInputPlaceholder')"
          :readonly="readonly"></v-textarea>
      </v-col>
      <v-col cols="12">
        <h4>{{ t('memory.create.perspective.title') }}</h4>
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
      <v-col cols="12" v-if="internalMemory.story">
        <v-textarea v-model="internalMemory.story" :label="t('memory.storyEditor.storyContent')" readonly></v-textarea>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, computed, type ComputedRef } from 'vue';
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
      emit('aiAnalysisCompleted'); // Signal to parent that analysis is done
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


// AI Event Suggestions (now derived from photoAnalysisResult or fallback)
const aiEventSuggestions: ComputedRef<string[]> = computed(() => { // Explicitly typed
  return internalMemory.value.photoAnalysisResult?.suggestions?.event || [
    t('memory.create.aiEventSuggestion.suggestion1'),
    t('memory.create.aiEventSuggestion.suggestion2'),
    t('memory.create.aiEventSuggestion.suggestion3'),
    t('memory.create.aiEventSuggestion.suggestion4'),
  ];
});

// AI Emotion & Context Suggestions (now derived from photoAnalysisResult or fallback)
const aiEmotionContextSuggestions: ComputedRef<string[]> = computed(() => { // Explicitly typed
  return internalMemory.value.photoAnalysisResult?.suggestions?.emotion || [
    t('memory.create.aiEmotionContextSuggestion.suggestion1'),
    t('memory.create.aiEmotionContextSuggestion.suggestion2'),
    t('memory.create.aiEmotionContextSuggestion.suggestion3'),
    t('memory.create.aiEmotionContextSuggestion.suggestion4'),
    t('memory.create.aiEmotionContextSuggestion.suggestion5'),
    t('memory.create.aiEmotionContextSuggestion.suggestion6'),
  ];
});

// AI Perspective Suggestions (remains fixed as no AI suggestions for this)
const aiPerspectiveSuggestions = ref([
  { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
  { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
  { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
]);

const validate = async () => {
  return formStep2.value ? (await formStep2.value.validate()).valid : false;
};

defineExpose({
  validate,
  triggerAiAnalysis,
});
</script>