<template>
  <v-card class="living-deceased-card" elevation="2" hover>
    <v-card-text>
      <div v-if="loading" class="text-center text-body-2 text-high-emphasis font-weight-bold">{{ t('dashboard.livingDeceasedCard.loading') }}</div>
      <div v-else-if="livingMembersCount === undefined || deceasedMembersCount === undefined"
        class="text-center text-body-2 font-weight-bold">
        {{ t('dashboard.livingDeceasedCard.noData') }}
      </div>
      <div v-else class="d-flex justify-space-between align-center">
        <div>
          <p class="text-subtitle-1 font-weight-bold text-high-emphasis">{{ t('dashboard.livingDeceasedCard.title') }}</p>
          <h4 class="text-h5 font-weight-bold">
            <span class="success--text">{{ livingMembersCount || 0 }}</span> /
            <span class="error--text">{{ deceasedMembersCount || 0 }}</span>
          </h4>
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

const { livingMembersCount, deceasedMembersCount, loading } = defineProps<{
  livingMembersCount: number | undefined;
  deceasedMembersCount: number | undefined;
  loading: boolean;
}>();
</script>

<style scoped>
.living-deceased-card {
  height: 105px;
}
</style>