<template>
  <v-select
    v-model="aiBiographyStore.selectedProvider"
    :items="providers"
    item-title="name"
    item-value="providerType"
    :label="t('aiBiography.input.providerLabel')"
    :loading="aiBiographyStore.loading"
    :error-messages="aiBiographyStore.error || []"
    variant="outlined"
    density="compact"
    hide-details="auto"
  >
    <template v-slot:item="{ props, item }">
      <v-list-item v-bind="props"
        :title="item.raw.name"
        :subtitle="item.raw.isEnabled ? t('aiBiography.input.providerEnabled') : t('aiBiography.input.providerDisabled')"
      >
        <template v-slot:append>
          <v-chip
            v-if="item.raw.isEnabled"
            color="success"
            size="x-small"
            class="ml-2"
          >{{ t('aiBiography.input.providerActive') }}</v-chip>
          <v-chip
            v-else
            color="error"
            size="x-small"
            class="ml-2"
          >{{ t('aiBiography.input.providerInactive') }}</v-chip>
        </template>
      </v-list-item>
    </template>
  </v-select>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAIBiographyStore } from '@/stores/aiBiography.store';
import type { AIProviderDto } from '@/types';

const { t } = useI18n();

const aiBiographyStore = useAIBiographyStore();

const providers = computed(() => {
  return aiBiographyStore.aiProviders.map((p: AIProviderDto) => ({
    name: p.name,
    providerType: p.providerType,
    isEnabled: p.isEnabled
  }));
});

onMounted(() => {
  aiBiographyStore.fetchAIProviders();
});
</script>
