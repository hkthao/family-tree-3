<template>
  <v-card class="mb-4">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-account-group</v-icon>
      <span class="ml-2">{{ t('dashboard.livingDeceasedCard.title') }}</span>
      <v-spacer></v-spacer>
      <v-progress-circular v-if="loading" indeterminate size="24" color="primary"></v-progress-circular>
    </v-card-title>
    <v-card-text>
      <div v-if="loading" class="text-center">{{ t('dashboard.livingDeceasedCard.loading') }}</div>
      <div v-else-if="livingMembersCount === undefined || deceasedMembersCount === undefined" class="text-center text-medium-emphasis">
        {{ t('dashboard.livingDeceasedCard.noData') }}
      </div>
      <v-row v-else class="text-center">
        <v-col cols="6">
          <div class="text-h5 font-weight-bold success--text">{{ livingMembersCount }}</div>
          <div class="text-subtitle-1 text-medium-emphasis">{{ t('dashboard.livingDeceasedCard.living') }}</div>
        </v-col>
        <v-col cols="6">
          <div class="text-h5 font-weight-bold error--text">{{ deceasedMembersCount }}</div>
          <div class="text-subtitle-1 text-medium-emphasis">{{ t('dashboard.livingDeceasedCard.deceased') }}</div>
        </v-col>
      </v-row>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps<{
  livingMembersCount: number | undefined;
  deceasedMembersCount: number | undefined;
  loading: boolean;
}>();
</script>

<style scoped>
/* Add any specific styles for the card here */
</style>
