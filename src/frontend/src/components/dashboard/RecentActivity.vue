<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-chart-timeline</v-icon>
      <span class="ml-2">{{ t('dashboard.recentActivity.title') }}</span>
    </v-card-title>
    <v-card-text class="scrollable-card-content">
      <div v-if="userActivityStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">{{ t('dashboard.recentActivity.loading') }}</p>
      </div>
      <v-alert v-else-if="userActivityStore.error" type="error" dense dismissible class="mb-4">
        {{ userActivityStore.error }}
      </v-alert>
      <v-timeline v-else density="compact" align="start" truncate-line="both">
        <v-timeline-item
          v-for="item in userActivityStore.items"
          :key="item.id"
          :dot-color="getDotColor(item.targetType)"
          size="small"
        >
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
        <template v-if="userActivityStore.items.length === 0">
          <v-timeline-item size="small">
            <template v-slot:opposite>
              <div class="text-caption"></div>
            </template>
            <div>
              <div class="font-weight-normal">{{ t('dashboard.recentActivity.noData') }}</div>
            </div>
          </v-timeline-item>
        </template>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useUserActivityStore } from '@/stores';
import { onMounted, watch } from 'vue';
import { TargetType } from '@/types';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  familyId: { type: String, default: null },
});

const userActivityStore = useUserActivityStore();

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

const fetchActivities = (familyId: string | null) => {
  userActivityStore.fetchRecentActivities(20, undefined, undefined, familyId || undefined);
};

onMounted(() => {
  fetchActivities(props.familyId);
});

watch(() => props.familyId, (newFamilyId) => {
  fetchActivities(newFamilyId);
});
</script>

<style scoped>
.scrollable-card-content {
  max-height: 400px; /* Adjust as needed */
  overflow-y: auto;
}
</style>
