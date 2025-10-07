<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-text-box-outline</v-icon>
      <span class="ml-2">{{ t('aiBiography.output.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="aiBiographyStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">{{ t('aiBiography.output.loading') }}</p>
      </div>
      <v-alert v-else-if="aiBiographyStore.error" type="error" dense dismissible class="mb-4">
        {{ aiBiographyStore.error }}
      </v-alert>
      <div v-else-if="aiBiographyStore.biographyResult">
        <v-textarea
          v-model="editableContent"
          :label="t('aiBiography.output.biographyContentLabel')"
          rows="10"
          variant="outlined"
          class="mb-4"
        ></v-textarea>
        <v-chip class="mr-2" size="small">
          {{ t('aiBiography.output.provider') }}: {{ aiBiographyStore.biographyResult.provider }}
        </v-chip>
        <v-chip size="small">
          {{ t('aiBiography.output.tokensUsed') }}: {{ aiBiographyStore.biographyResult.tokensUsed }}
        </v-chip>
        <v-btn color="primary" class="mt-4" @click="saveBiography">
          {{ t('aiBiography.output.saveButton') }}
        </v-btn>
        <v-btn color="secondary" class="mt-4 ml-2" @click="regenerateBiography">
          {{ t('aiBiography.output.regenerateButton') }}
        </v-btn>
      </div>
      <div v-else>
        <p>{{ t('aiBiography.output.noBiographyYet') }}</p>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/aiBiography.store';

const { t } = useI18n();
const aiBiographyStore = useAIBiographyStore();

const editableContent = ref('');

watch(() => aiBiographyStore.biographyResult, (newResult) => {
  if (newResult) {
    editableContent.value = newResult.content;
  }
}, { immediate: true });

const saveBiography = () => {
  // TODO: Implement actual save logic to member profile
  console.log('Saving biography:', editableContent.value);
};

const regenerateBiography = () => {
  aiBiographyStore.generateBiography();
};
</script>
