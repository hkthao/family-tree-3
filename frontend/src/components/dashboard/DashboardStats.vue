<template>
  <div>
    <div v-if="dashboardStore.loading">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">Đang tải thống kê...</p>
    </div>
    <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible >
      {{ dashboardStore.error }}
    </v-alert>
    <v-row v-else-if="dashboardStore.stats">
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="blue">mdi-account-group</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.stats.totalFamilies || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Gia đình</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="green">mdi-account-multiple</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.stats.totalMembers || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Thành viên</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="purple">mdi-link-variant</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.stats.totalRelationships || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Mối quan hệ</div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-3 text-center" elevation="1">
          <v-icon size="40" color="orange">mdi-sitemap</v-icon>
          <div class="text-h5 font-weight-bold mt-2">{{ dashboardStore.stats.totalGenerations || 0 }}</div>
          <div class="text-subtitle-1 text-grey">Thế hệ</div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted, watch } from 'vue';

const props = defineProps({
  familyId: { type: String, default: null },
});

const dashboardStore = useDashboardStore();
const fetchStats = (familyId: string | null) => {
  dashboardStore.fetchDashboardStats(familyId || undefined);
};

onMounted(() => {
  fetchStats(props.familyId);
});

watch(() => props.familyId, (newFamilyId) => {
  fetchStats(newFamilyId);
});
</script>

<style scoped>
/* Add any specific styles for DashboardStats here */
</style>
