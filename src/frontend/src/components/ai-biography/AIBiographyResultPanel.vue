<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/ai-biography.store';

const props = defineProps({
  biographyContent: { type: String, default: '' },
});

const emit = defineEmits(['update:biographyContent', 'save', 'regenerate']);

const { t } = useI18n();
const aiBiographyStore = useAIBiographyStore();

const displayContent = computed({
  get: () => props.biographyContent,
  set: (value) => emit('update:biographyContent', value),
});

const saveBiography = () => {
  emit('save', props.biographyContent);
};

const regenerateBiography = () => {
  emit('regenerate');
};
</script>

<template>
  <v-card class="pa-4 d-flex flex-column" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-text-box-outline</v-icon>
      <span class="ml-2">{{ t('aiBiography.output.title') }}</span>
    </v-card-title>
    <v-card-text class="flex-1">
      <div v-if="aiBiographyStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">{{ t('aiBiography.output.loading') }}</p>
      </div>
      <v-alert v-else-if="aiBiographyStore.error" type="error" dense dismissible class="mb-4">
        {{ aiBiographyStore.error }}
      </v-alert>
      <div v-else-if="aiBiographyStore.biographyResult || biographyContent">
        <v-textarea v-model="displayContent" :label="t('aiBiography.output.biographyContentLabel')" auto-grow
          variant="outlined"></v-textarea>
      </div>
      <div v-else>
        <p>{{ t('aiBiography.output.noBiographyYet') }}</p>
      </div>
    </v-card-text>

    <v-card-actions v-if="aiBiographyStore.biographyResult || biographyContent">
      <v-btn color="primary" @click="saveBiography">
        {{ t('aiBiography.output.saveButton') }}
      </v-btn>
      <v-btn color="secondary" class="ml-2" @click="regenerateBiography">
        {{ t('aiBiography.output.regenerateButton') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>
