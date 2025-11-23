<template>
  <v-card class="rounded-xl" elevation="2" style="transition: all 0.3s ease-in-out;" hover>
    <v-card-text>
      <div v-if="loading" class="text-center py-2">{{ t('dashboard.livingDeceasedCard.loading') }}</div>
      <div v-else-if="livingMembersCount === undefined || deceasedMembersCount === undefined" class="text-center text-medium-emphasis py-2">
        {{ t('dashboard.livingDeceasedCard.noData') }}
      </div>
      <div v-else class="d-flex justify-space-between align-center py-2">
        <div>
          <p class="text-caption">{{ t('dashboard.livingDeceasedCard.living') }}</p>
          <h4 class="text-h5 font-weight-bold success--text">{{ livingMembersCount }}</h4>
        </div>
        <div>
          <p class="text-caption">{{ t('dashboard.livingDeceasedCard.deceased') }}</p>
          <h4 class="text-h5 font-weight-bold error--text">{{ deceasedMembersCount }}</h4>
        </div>
        <v-avatar color="light-blue-darken-1" rounded="lg" size="42">
          <v-icon icon="mdi-account-group" color="white"></v-icon>
        </v-avatar>
      </div>
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
