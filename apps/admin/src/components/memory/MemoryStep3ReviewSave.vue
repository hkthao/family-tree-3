<template>
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
    <v-col cols="12" v-if="memoryStore.faceRecognition.uploadedImage">
      <h4>{{ t('memory.create.step1.title') }}</h4> <!-- Photo Upload Title for review -->
      <img :src="memoryStore.faceRecognition.uploadedImage" height="100" class="ma-2" />
      <p v-if="!props.readonly">{{ t('memory.create.step2.analysisResult') }}</p>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemoryDto } from '@/types/memory';
import { useMemoryStore } from '@/stores/memory.store'; // Use the main memory store

const props = defineProps<{
  modelValue: MemoryDto;
  readonly?: boolean;
}>();

const { t } = useI18n();
const memoryStore = useMemoryStore(); // Initialize the main memory store

const internalMemory = computed<MemoryDto>({
  get: () => props.modelValue,
  set: (value: MemoryDto) => { /* No-op for review step */ },
});
</script>