<template>
  <div>
    <div v-if="dashboardStore.loading">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">Đang tải thống kê...</p>
    </div>
    <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible class="mb-4">
      {{ dashboardStore.error }}
    </v-alert>
    <v-row v-else-if="dashboardStore.dashboardData.stats">
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="blue">mdi-account-group</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.dashboardData.stats.totalFamilies || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Gia đình</div>
          <div v-if="dashboardStore.dashboardData.stats.familyTrend" :class="dashboardStore.dashboardData.stats.familyTrend.isPositiveTrend ? 'text-green' : 'text-red'">
            <v-icon small>{{ dashboardStore.dashboardData.stats.familyTrend.isPositiveTrend ? 'mdi-arrow-up' : 'mdi-arrow-down' }}</v-icon>
            {{ dashboardStore.dashboardData.stats.familyTrend.changePercentage }}%
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="green">mdi-account-multiple</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.dashboardData.stats.totalMembers || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Thành viên</div>
          <div v-if="dashboardStore.dashboardData.stats.memberTrend" :class="dashboardStore.dashboardData.stats.memberTrend.isPositiveTrend ? 'text-green' : 'text-red'">
            <v-icon small>{{ dashboardStore.dashboardData.stats.memberTrend.isPositiveTrend ? 'mdi-arrow-up' : 'mdi-arrow-down' }}</v-icon>
            {{ dashboardStore.dashboardData.stats.memberTrend.changePercentage }}%
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="purple">mdi-link-variant</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.dashboardData.stats.totalRelationships || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Mối quan hệ</div>
          <div v-if="dashboardStore.dashboardData.stats.relationshipTrend" :class="dashboardStore.dashboardData.stats.relationshipTrend.isPositiveTrend ? 'text-green' : 'text-red'">
            <v-icon small>{{ dashboardStore.dashboardData.stats.relationshipTrend.isPositiveTrend ? 'mdi-arrow-up' : 'mdi-arrow-down' }}</v-icon>
            {{ dashboardStore.dashboardData.stats.relationshipTrend.changePercentage }}%
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="orange">mdi-sitemap</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.dashboardData.stats.totalGenerations || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Thế hệ</div>
          <div v-if="dashboardStore.dashboardData.stats.generationTrend" :class="dashboardStore.dashboardData.stats.generationTrend.isPositiveTrend ? 'text-green' : 'text-red'">
            <v-icon small>{{ dashboardStore.dashboardData.stats.generationTrend.isPositiveTrend ? 'mdi-arrow-up' : 'mdi-arrow-down' }}</v-icon>
            {{ dashboardStore.dashboardData.stats.generationTrend.changePercentage }}%
          </div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted } from 'vue';

const dashboardStore = useDashboardStore();

onMounted(() => {
  dashboardStore.fetchDashboardStats();
});
</script>

<style scoped>
/* Add any specific styles for DashboardStats here */
</style>