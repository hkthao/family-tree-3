<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-pencil-box-multiple-outline</v-icon>
      <span class="ml-2">{{ t('aiBiography.input.title') }}</span>
    </v-card-title>
    <v-card-text>
      <v-textarea
        v-model="aiBiographyStore.userPrompt"
        :label="t('aiBiography.input.promptLabel')"
        :placeholder="t('aiBiography.input.promptPlaceholder')"
        rows="5"
        variant="outlined"
        clearable
        class="mb-4"
      ></v-textarea>

      <v-switch
        v-model="aiBiographyStore.savePromptForLater"
        :label="t('aiBiography.input.savePromptLabel')"
        color="primary"
        hide-details
        class="mb-4"
      ></v-switch>

      <v-radio-group v-model="aiBiographyStore.useDBData" inline class="mb-4">
        <v-radio :label="t('aiBiography.input.autoMode')" :value="true"></v-radio>
        <v-radio :label="t('aiBiography.input.manualMode')" :value="false"></v-radio>
      </v-radio-group>

      <v-select
        v-model="aiBiographyStore.style"
        :items="biographyStyles"
        :label="t('aiBiography.input.styleLabel')"
        item-title="text"
        item-value="value"
        variant="outlined"
        density="compact"
        class="mb-4"
      ></v-select>

      <AIBiographyProviderSelect class="mb-4" />

      <v-text-field
        v-model.number="aiBiographyStore.maxTokens"
        :label="t('aiBiography.input.tokenLimitLabel')"
        type="number"
        variant="outlined"
        density="compact"
        class="mb-4"
      ></v-text-field>

      <v-slider
        v-model="aiBiographyStore.temperature"
        :label="t('aiBiography.input.temperatureLabel')"
        :min="0"
        :max="1"
        :step="0.1"
        thumb-label
        class="mb-4"
      ></v-slider>

      <v-btn
        color="primary"
        class="mr-2"
        :loading="aiBiographyStore.loading"
        @click="aiBiographyStore.generateBiography()"
      >
        {{ t('aiBiography.input.generateButton') }}
      </v-btn>
      <v-btn color="grey" class="mr-2" @click="aiBiographyStore.clearForm()">
        {{ t('aiBiography.input.clearButton') }}
      </v-btn>
      <v-btn color="info" @click="aiBiographyStore.useSavedPrompt(aiBiographyStore.memberId!)">
        {{ t('aiBiography.input.useSavedPromptButton') }}
      </v-btn>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/aiBiography.store';
import { BiographyStyle } from '@/types';
import AIBiographyProviderSelect from './AIBiographyProviderSelect.vue';

const { t } = useI18n();
const aiBiographyStore = useAIBiographyStore();

// Expose memberId prop to the store
const props = defineProps({
  memberId: { type: String, required: true },
});

// Update store's memberId when prop changes
watch(() => props.memberId, (newId) => {
  aiBiographyStore.memberId = newId;
  aiBiographyStore.fetchLastUserPrompt(newId);
});

onMounted(() => {
  aiBiographyStore.memberId = props.memberId;
  aiBiographyStore.fetchLastUserPrompt(props.memberId);
});

const biographyStyles = computed(() => [
  { text: t('aiBiography.styles.emotional'), value: BiographyStyle.Emotional },
  { text: t('aiBiography.styles.historical'), value: BiographyStyle.Historical },
  { text: t('aiBiography.styles.storytelling'), value: BiographyStyle.Storytelling },
  { text: t('aiBiography.styles.formal'), value: BiographyStyle.Formal },
  { text: t('aiBiography.styles.informal'), value: BiographyStyle.Informal },
]);
</script>
