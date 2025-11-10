<template>
  <v-card :elevation="0">
    <v-card-title class="text-h5 text-uppercase d-flex justify-space-between align-center">
      <span>{{ t('naturalLanguage.title') }}</span>
      <v-btn icon variant="text" @click="closeDrawer">
        <v-icon>mdi-close</v-icon>
      </v-btn>
    </v-card-title>
    <v-card-text>
      <v-textarea
        v-model="nlStore.input"
        :label="t('naturalLanguage.inputLabel')"
        :placeholder="t('naturalLanguage.inputPlaceholder')"
        rows="4"
        auto-grow
        variant="outlined"
        class="mb-4"
        :loading="nlStore.loading"
        :disabled="nlStore.loading"
      ></v-textarea>

      <v-btn
        color="primary"
        block
        class="mb-4"
        :loading="nlStore.loading"
        :disabled="nlStore.loading || !nlStore.input.trim()"
        @click="parseInput"
      >
        {{ t('naturalLanguage.parseButton') }}
      </v-btn>

      <v-alert v-if="nlStore.error" type="error" class="mb-4">{{ nlStore.error }}</v-alert>

      <v-divider class="my-4"></v-divider>

      <h3 class="text-h6 mb-2">{{ t('naturalLanguage.parsedDataPreview') }}</h3>
      <v-progress-linear v-if="nlStore.loading" indeterminate color="primary" class="mb-4"></v-progress-linear>

      <div v-if="nlStore.parsedData && nlStore.entityType">
        <ParsedDataCard
          :parsed-data="nlStore.parsedData"
          :entity-type="nlStore.entityType"
          @save="applyParsedData"
          @cancel="nlStore.clearState"
        />
      </div>
      <v-alert v-else-if="!nlStore.loading && nlStore.input.trim()" type="info" class="mb-4">
        {{ t('naturalLanguage.noParsedData') }}
      </v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import ParsedDataCard from './ParsedDataCard.vue';
import type { Member, Event, Family, Relationship } from '@/types';

interface NLInputDrawerProps {
  modelValue: boolean; // Controls drawer visibility
}

const props = defineProps<NLInputDrawerProps>();
const emit = defineEmits(['update:modelValue', 'apply']);

const { t } = useI18n();
const nlStore = useNaturalLanguageStore();
const router = useRouter();

const parseInput = async () => {
  await nlStore.parseInput();
};

const applyParsedData = () => {
  if (nlStore.parsedData) {
    // The logic to save the data will be handled in the confirmation view
    router.push({ name: 'NLConfirmation' });
  }
};

const closeDrawer = () => {
  emit('update:modelValue', false);
  nlStore.clearState();
};

watch(
  () => props.modelValue,
  (newVal) => {
    if (!newVal) {
      // When drawer closes, clear store state
      nlStore.clearState();
    }
  },
);
</script>
