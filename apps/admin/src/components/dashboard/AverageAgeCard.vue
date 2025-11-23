<template>
  <v-card class="mb-4">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-account-clock-outline</v-icon>
      <span class="ml-2">{{ t('dashboard.averageAgeCard.title') }}</span>
      <v-spacer></v-spacer>
      <v-progress-circular v-if="loading" indeterminate size="24" color="primary"></v-progress-circular>
    </v-card-title>
    <v-card-text>
      <div v-if="loading" class="text-center">{{ t('dashboard.averageAgeCard.loading') }}</div>
      <div v-else-if="averageAge === undefined" class="text-center text-medium-emphasis">
        {{ t('dashboard.averageAgeCard.noData') }}
      </div>
      <div v-else class="text-center text-h4 font-weight-bold">
        {{ averageAge.toFixed(1) }} {{ t('dashboard.averageAgeCard.years') }}
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps<{
  averageAge: number | undefined;
  loading: boolean;
}>();
</script>

<style scoped>
/* Add any specific styles for the card here */
</style>
