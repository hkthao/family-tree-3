<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-chart-timeline</v-icon>
      <span class="ml-2">{{ t('dashboard.recentActivity.title') }}</span>
    </v-card-title>
    <v-card-text class="scrollable-card-content">
      <v-progress-linear v-if="userActivityStore.loading" indeterminate class="mb-4" ></v-progress-linear>
      <v-alert v-if="userActivityStore.error" type="error" dense dismissible class="mb-4">
        {{ userActivityStore.error }}
      </v-alert>
      <v-timeline v-else align="start" truncate-line="both">
        <v-timeline-item v-for="item in userActivityStore.items" :key="item.id"
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
        <template v-if="userActivityStore.items.length === 0 && !userActivityStore.loading">
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
    <v-card-actions class="justify-center">
      <v-pagination v-if="userActivityStore.totalPages > 1" :model-value="userActivityStore.page"
        :length="userActivityStore.totalPages" :total-visible="4" @update:modelValue="handlePageChange"></v-pagination>
    </v-card-actions>
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

const fetchActivities = (page: number, familyId: string | null) => {
  userActivityStore.fetchRecentActivities(10, undefined, undefined, familyId || undefined, page);
};

const handlePageChange = (page: number) => {
  fetchActivities(page, props.familyId);
};

onMounted(() => {
  fetchActivities(1, props.familyId);
});

watch(() => props.familyId, (newFamilyId) => {
  fetchActivities(1, newFamilyId);
});
</script>

<style scoped>
.scrollable-card-content {
  max-height: 400px;
  /* Adjust as needed */
  overflow-y: auto;
}
</style>
