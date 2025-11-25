<template>
  <v-form ref="formStep2">
    <v-row>
      <v-col cols="12">
        <MemberAutocomplete class="mt-2" v-model="internalMemory.memberId" :label="t('member.form.member')"
          :rules="readonly ? [] : [(v: string) => !!v || t('common.validations.required')]"
          :readonly="readonly || !!memberId" required></MemberAutocomplete>
      </v-col>

      <v-col cols="12">
        <h4>{{ t('memory.create.aiCharacterSuggestion.title') }}</h4>
        <v-row v-if="internalMemory.faces && internalMemory.faces.length > 0">
          <v-col v-for="(face, index) in internalMemory.faces" :key="face.id" cols="6">
            <v-card>
              <v-img class="rounded-sm my-4" :src="getFaceThumbnailSrc(face)" height="100px" contain></v-img>
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
        <h4>{{ t('memory.create.aiEventSuggestion.title') }}</h4>
        <v-chip-group :model-value="internalMemory.eventSuggestion" @update:model-value="(newValue) => emit('update:modelValue', { ...props.modelValue, eventSuggestion: newValue })" color="primary" mandatory column :disabled="readonly">
          <v-chip v-for="event in aiEventSuggestions" :key="event" :value="event" filter variant="tonal">
            {{ event }}
          </v-chip>
          <v-chip value="unsure" filter variant="tonal">
            {{ t('memory.create.aiEventSuggestion.unsure') }}
          </v-chip>
        </v-chip-group>
        <v-text-field class="mt-2" v-if="internalMemory.eventSuggestion === 'unsure'"
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
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberAutocomplete } from '@/components/common';
import type { MemoryDto } from '@/types/memory';
import type { DetectedFace } from '@/types';

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
  memberId?: string | null;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const formStep2 = ref<HTMLFormElement | null>(null);

const internalMemory = computed<MemoryDto>({
  get: () => {
    const model = props.modelValue;
    return {
      ...model,
      rawInput: model.rawInput ?? undefined,
      story: model.story ?? undefined,
      eventSuggestion: model.eventSuggestion ?? aiEventSuggestions.value[0],
      customEventDescription: model.customEventDescription ?? undefined,
      emotionContextTags: model.emotionContextTags ?? [],
      customEmotionContext: model.customEmotionContext ?? undefined,
      faces: model.faces ?? [],
      perspective: model.perspective ?? aiPerspectiveSuggestions.value[0].value,
    } as MemoryDto;
  },
  set: (value: MemoryDto) => emit('update:modelValue', value),
});

watch(() => props.memberId, (newMemberId) => {
  if (newMemberId && !internalMemory.value.memberId) {
    internalMemory.value.memberId = newMemberId;
  }
}, { immediate: true });

// AI Event Suggestions
const aiEventSuggestions = ref([
  t('memory.create.aiEventSuggestion.suggestion1'),
  t('memory.create.aiEventSuggestion.suggestion2'),
  t('memory.create.aiEventSuggestion.suggestion3'),
  t('memory.create.aiEventSuggestion.suggestion4'),
]);

// AI Emotion & Context Suggestions
const aiEmotionContextSuggestions = ref([
  t('memory.create.aiEmotionContextSuggestion.suggestion1'),
  t('memory.create.aiEmotionContextSuggestion.suggestion2'),
  t('memory.create.aiEmotionContextSuggestion.suggestion3'),
  t('memory.create.aiEmotionContextSuggestion.suggestion4'),
  t('memory.create.aiEmotionContextSuggestion.suggestion5'),
  t('memory.create.aiEmotionContextSuggestion.suggestion6'),
]);

// AI Perspective Suggestions
const aiPerspectiveSuggestions = ref([
  { value: 'firstPerson', text: t('memory.create.perspective.firstPerson') },
  { value: 'neutralPersonal', text: t('memory.create.perspective.neutralPersonal') },
  { value: 'fullyNeutral', text: t('memory.create.perspective.fullyNeutral') },
]);

const getFaceThumbnailSrc = (face: DetectedFace) => {
  if (face.thumbnail) {
    return `data:image/jpeg;base64,${face.thumbnail}`;
  }
  return '';
};

const validate = async () => {
  return formStep2.value ? (await formStep2.value.validate()).valid : false;
};

defineExpose({
  validate,
});
</script>