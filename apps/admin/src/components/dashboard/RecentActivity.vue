<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-chart-timeline</v-icon>
      <span class="ml-2">{{ t('dashboard.recentActivity.title') }}</span>
    </v-card-title>
    <v-card-text class="scrollable-card-content">
      <v-progress-linear v-if="loading" indeterminate class="mb-4" ></v-progress-linear>
      <v-alert v-else-if="!activities || activities.length === 0" type="info" dense class="mb-4">
        {{ t('dashboard.recentActivity.noData') }}
      </v-alert>
      <v-timeline v-else align="start" truncate-line="both">
        <v-timeline-item v-for="item in activities" :key="item.id"
          :dot-color="getDotColor(item.targetType)" size="small">
          <template v-slot:opposite>
            <div class="text-caption">{{ new Date(item.created).toLocaleDateString() }}</div>
          </template>
          <div>
            <div class="font-weight-normal">
              <strong>{{ item.activitySummary }}</strong>
            </div>
            <div class="text-caption">{{ new Date(item.created).toLocaleTimeString() }}</div>
          </div>
        </v-timeline-item>
      </v-timeline>

    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { TargetType, type RecentActivity } from '@/types';
import { useI18n } from 'vue-i18n';
import type { PropType } from 'vue';

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, default: null }, // Keep familyId if it's used elsewhere for context
  activities: { type: Array as PropType<RecentActivity[]>, default: () => [] },
  loading: { type: Boolean, default: false },
});

const getDotColor = (targetType: TargetType) => {
  switch (targetType) {
    case TargetType.Family:
      return 'blue';
    case TargetType.Member:
      return 'green';
    case TargetType.Event:
      return 'purple';
    case TargetType.UserProfile:
      return 'orange';
    default:
      return 'grey';
  }
};
</script>

<style scoped>
.scrollable-card-content {
  max-height: 400px;
  /* Adjust as needed */
  overflow-y: auto;
}
</style>
