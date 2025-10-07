<template>
  <v-card class="pa-4" elevation="2">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-information</v-icon>
      <span class="ml-2">Trạng thái hệ thống</span>
    </v-card-title>
    <v-card-text>
      <div v-if="dashboardStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">Đang tải thông tin hệ thống...</p>
      </div>
      <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible class="mb-4">
        {{ dashboardStore.error }}
      </v-alert>
      <div v-else>
        <v-list dense>
          <v-list-item>
            <v-list-item-title>Trạng thái API:</v-list-item-title>
            <v-list-item-subtitle>
              <v-chip
                :color="dashboardStore.dashboardData.systemInfo?.apiStatus === 'online' ? 'green' : 'red'"
                small
              >
                {{ dashboardStore.dashboardData.systemInfo?.apiStatus === 'online' ? 'Trực tuyến' : 'Ngoại tuyến' }}
              </v-chip>
            </v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>Phiên bản ứng dụng:</v-list-item-title>
            <v-list-item-subtitle>{{ dashboardStore.dashboardData.systemInfo?.appVersion || 'N/A' }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>Lần đồng bộ cuối:</v-list-item-title>
            <v-list-item-subtitle>{{ dashboardStore.dashboardData.systemInfo?.lastSync ? new Date(dashboardStore.dashboardData.systemInfo.lastSync).toLocaleString() : 'N/A' }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>Thời gian máy chủ:</v-list-item-title>
            <v-list-item-subtitle>{{ dashboardStore.dashboardData.systemInfo?.serverTime ? new Date(dashboardStore.dashboardData.systemInfo.serverTime).toLocaleString() : 'N/A' }}</v-list-item-subtitle>
          </v-list-item>
        </v-list>
        <v-divider class="my-4"></v-divider>
        <div class="text-subtitle-1 mb-2">Tỷ lệ yêu cầu thành công:</div>
        <div class="d-flex justify-center">
          <!-- Chart removed temporarily -->
        </div>
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted, computed } from 'vue';
// Chart imports removed temporarily

const dashboardStore = useDashboardStore();

onMounted(() => {
  dashboardStore.fetchSystemInfo();
});

// Chart data and options removed temporarily
const chartData = computed(() => null);
const chartOptions = {};
</script>

<style scoped>
/* Add any specific styles for SystemStatus here */
</style>