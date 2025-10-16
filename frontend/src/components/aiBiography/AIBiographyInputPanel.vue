<template>
  <v-card class="pa-4 d-flex flex-column" elevation="2" height="100%">
    <v-card-text class="flex-1">
      <v-select v-model="aiBiographyStore.style" :items="biographyStyles" :label="t('aiBiography.input.styleLabel')"
        item-title="text" item-value="value" variant="outlined" density="compact" :hide-details="true"></v-select>
      <v-checkbox v-model="aiBiographyStore.generatedFromDB" :label="t('aiBiography.input.useSystemData')"
        :hide-details="true"></v-checkbox>
      <v-textarea v-model="aiBiographyStore.userPrompt" :label="t('aiBiography.input.promptLabel')"
        :placeholder="t('aiBiography.input.promptPlaceholder')" rows="5" variant="outlined" clearable counter
        :rules="[rules.userPromptLength]"></v-textarea>
    </v-card-text>

    <v-card-actions>
      <v-btn color="primary" class="mr-2" :loading="aiBiographyStore.loading"
        @click="aiBiographyStore.generateBiography()">
        {{ t('aiBiography.input.generateButton') }}
      </v-btn>
      <v-btn color="grey" class="mr-2" @click="aiBiographyStore.clearForm()">
        {{ t('aiBiography.input.clearButton') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/aiBiography.store';
import { BiographyStyle } from '@/types';


const { t } = useI18n();
const aiBiographyStore = useAIBiographyStore();

const biographyStyles = computed(() => [
  { text: t('aiBiography.styles.emotional'), value: BiographyStyle.Emotional },
  { text: t('aiBiography.styles.historical'), value: BiographyStyle.Historical },
  { text: t('aiBiography.styles.storytelling'), value: BiographyStyle.Storytelling },
  { text: t('aiBiography.styles.formal'), value: BiographyStyle.Formal },
  { text: t('aiBiography.styles.informal'), value: BiographyStyle.Informal },
]);

const rules = {
  userPromptLength: (value: string) => (value && value.length <= 1000) || t('aiBiography.input.validation.userPromptLength'),
};
</script>
