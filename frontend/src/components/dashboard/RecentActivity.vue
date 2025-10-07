<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-chart-timeline</v-icon>
      <span class="ml-2">Hoạt động gần đây</span>
    </v-card-title>
    <v-card-text class="scrollable-card-content">
      <div v-if="dashboardStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">Đang tải hoạt động gần đây...</p>
      </div>
      <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible class="mb-4">
        {{ dashboardStore.error }}
      </v-alert>
      <v-timeline v-else density="compact" align="start" truncate-line="both">
        <v-timeline-item
          v-for="item in dashboardStore.dashboardData.recentActivity"
          :key="item.id"
          :dot-color="item.type === 'member' ? 'blue' : item.type === 'relationship' ? 'green' : 'purple'"
          size="small"
        >
          <template v-slot:opposite>
            <div class="text-caption">{{ new Date(item.timestamp).toLocaleDateString() }}</div>
          </template>
          <div>
            <div class="font-weight-normal">
              <strong>{{ item.description }}</strong>
            </div>
            <div class="text-caption">{{ new Date(item.timestamp).toLocaleTimeString() }}</div>
          </div>
        </v-timeline-item>
        <v-timeline-item v-if="dashboardStore.dashboardData.recentActivity.length === 0" size="small">
          <template v-slot:opposite>
            <div class="text-caption"></div>
          </template>
          <div>
            <div class="font-weight-normal">Không có hoạt động gần đây.</div>
          </div>
        </v-timeline-item>
      </v-timeline>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted } from 'vue';

const dashboardStore = useDashboardStore();

onMounted(() => {
  dashboardStore.fetchRecentActivity();
});
</script>

<style scoped>
.scrollable-card-content {
  max-height: 400px; /* Adjust as needed */
  overflow-y: auto;
}
</style>
