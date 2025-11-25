<template>
  <v-row>
    <v-col cols="12">
      <h4>{{ t('memory.create.aiEventSuggestion.title') }}</h4>
      <v-chip-group v-model="localSuggestions.eventSuggestion" color="primary" mandatory column>
        <v-chip v-for="event in aiEventSuggestions" :key="event" :value="event" filter variant="tonal">
          {{ event }}
        </v-chip>
        <v-chip value="unsure" filter variant="tonal">
          {{ t('memory.create.aiEventSuggestion.unsure') }}
        </v-chip>
      </v-chip-group>
      <v-text-field v-if="localSuggestions.eventSuggestion === 'unsure'"
        v-model="localSuggestions.customEventDescription"
        :label="t('memory.create.aiEventSuggestion.customDescription')" clearable :readonly="readonly"></v-text-field>
    </v-col>

    <v-col cols="12">
      <h4>{{ t('memory.create.aiCharacterSuggestion.title') }}</h4>
      <v-row v-if="detectedFaces.length > 0">
        <v-col v-for="(face, index) in detectedFaces" :key="face.id" cols="12">
          <v-card >
            <v-img :src="face.photoUrl" height="100px" contain></v-img>
            <MemberAutocomplete v-model="face.memberId" :label="t('member.form.member')" :readonly="readonly"
              variant="outlined"></MemberAutocomplete>
            <v-text-field v-model="face.relationPrompt" :label="t('memory.create.aiCharacterSuggestion.relationPrompt')"
              :readonly="readonly"></v-text-field>
          </v-card>
        </v-col>
      </v-row>
      <p v-else class="text-body-2 text-grey">{{ t('memory.create.aiCharacterSuggestion.noFacesDetected') }}</p>
    </v-col>

    <v-col cols="12">
      <h4>{{ t('memory.create.aiEmotionContextSuggestion.title') }}</h4>
      <v-chip-group v-model="localSuggestions.emotionContextTags" multiple filter color="primary" column>
        <v-chip v-for="tag in aiEmotionContextSuggestions" :key="tag" :value="tag" filter variant="tonal">
          {{ tag }}
        </v-chip>
      </v-chip-group>
      <v-text-field v-model="localSuggestions.customEmotionContext"
        :label="t('memory.create.aiEmotionContextSuggestion.customInput')" clearable
        :readonly="readonly"></v-text-field>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberAutocomplete } from '@/components/common';
import type { MemoryDto } from '@/types/memory';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const localSuggestions = ref<MemoryDto>({
  id: undefined,
  memberId: '',
  title: '',
  rawInput: undefined, // Changed from story to rawInput and made optional
  story: undefined, // Added new field
  eventSuggestion: undefined,
  customEventDescription: undefined,
  emotionContextTags: [],
  customEmotionContext: undefined,
  faces: [],
  createdAt: undefined,
});

const updatingFromProp = ref(false); // Flag to prevent recursive updates

// Watch props.modelValue to update localSuggestions when parent changes
watch(() => props.modelValue, (newVal) => {
  updatingFromProp.value = true; // Set flag to indicate update is from prop

  // Update properties individually to avoid reassigning the whole object
  localSuggestions.value.id = newVal.id;
  localSuggestions.value.memberId = newVal.memberId;
  localSuggestions.value.title = newVal.title;
  localSuggestions.value.rawInput = newVal.rawInput; // Use rawInput
  localSuggestions.value.story = newVal.story; // Add story
  localSuggestions.value.photoAnalysisId = newVal.photoAnalysisId;
  localSuggestions.value.photoUrl = newVal.photoUrl;
  localSuggestions.value.tags = newVal.tags ?? [];
  localSuggestions.value.keywords = newVal.keywords ?? [];
  localSuggestions.value.eventSuggestion = newVal.eventSuggestion ?? undefined;
  localSuggestions.value.customEventDescription = newVal.customEventDescription ?? undefined;
  localSuggestions.value.emotionContextTags = newVal.emotionContextTags ?? [];
  localSuggestions.value.customEmotionContext = newVal.customEmotionContext ?? undefined;
  localSuggestions.value.faces = newVal.faces ?? [];
  localSuggestions.value.createdAt = newVal.createdAt;
  localSuggestions.value.photoAnalysisResult = newVal.photoAnalysisResult;

  nextTick(() => {
    updatingFromProp.value = false;
  });
}, { immediate: true, deep: true });

watch(localSuggestions, (newVal) => {
  if (!updatingFromProp.value) {
    emit('update:modelValue', newVal);
  }
}, { deep: true });

// AI Event Suggestions
const aiEventSuggestions = ref([
  t('memory.create.aiEventSuggestion.suggestion1'),
  t('memory.create.aiEventSuggestion.suggestion2'),
  t('memory.create.aiEventSuggestion.suggestion3'),
  t('memory.create.aiEventSuggestion.suggestion4'),
]);

interface DetectedFace {
  id: string;
  photoUrl: string; // Base64 or object URL of the cropped face
  memberId: string | null; // User's selected member ID
  relationPrompt: string; // User input for "who is this?" or "relation?"
}

const detectedFaces = ref<DetectedFace[]>([]);

// Watch localSuggestions.faces to populate detectedFaces (e.g., when editing an existing memory)
watch(() => localSuggestions.value.faces, (newFaces) => {
  if (newFaces) {
    detectedFaces.value = newFaces.map(faceDto => ({
      id: faceDto.faceId || '',
      photoUrl: '',
      memberId: faceDto.memberId,
      relationPrompt: faceDto.relationPrompt || '',
    }));
  } else {
    detectedFaces.value = [];
  }
}, { immediate: true, deep: true });

// Watch detectedFaces to update localSuggestions.faces when changes occur in UI
watch(detectedFaces, (newDetectedFaces) => {
  localSuggestions.value.faces = newDetectedFaces.map(face => ({
    faceId: face.id,
    memberId: face.memberId,
    relationPrompt: face.relationPrompt,
  }));
}, { deep: true });


// AI Emotion & Context Suggestions
const aiEmotionContextSuggestions = ref([
  t('memory.create.aiEmotionContextSuggestion.suggestion1'),
  t('memory.create.aiEmotionContextSuggestion.suggestion2'),
  t('memory.create.aiEmotionContextSuggestion.suggestion3'),
  t('memory.create.aiEmotionContextSuggestion.suggestion4'),
  t('memory.create.aiEmotionContextSuggestion.suggestion5'),
  t('memory.create.aiEmotionContextSuggestion.suggestion6'),
]);

</script>
