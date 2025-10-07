<template>
  <v-card class="pa-4" elevation="2" height="100%">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-calendar-clock</v-icon>
      <span class="ml-2">Sự kiện sắp tới</span>
    </v-card-title>
    <v-card-text class="scrollable-card-content">
      <div v-if="dashboardStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">Đang tải sự kiện sắp tới...</p>
      </div>
      <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible class="mb-4">
        {{ dashboardStore.error }}
      </v-alert>
      <v-list v-else dense>
        <v-list-item v-for="(item, index) in dashboardStore.upcomingEvents" :key="index">
          <template v-slot:prepend>
            <v-icon color="blue">mdi-calendar-check</v-icon>
          </template>
          <v-list-item-title>{{ item.name }}</v-list-item-title>
          <v-list-item-subtitle>
            {{ item.startDate ? new Date(item.startDate).toLocaleDateString() : 'N/A' }} - {{ item.endDate ? new Date(item.endDate).toLocaleDateString() : 'N/A' }}
          </v-list-item-subtitle>
        </v-list-item>
        <v-list-item v-if="dashboardStore.upcomingEvents.length === 0">
          <v-list-item-title>Không có sự kiện sắp tới.</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted, watch } from 'vue';

const props = defineProps({
  familyId: { type: String, default: null },
});

const dashboardStore = useDashboardStore();

const fetchEvents = (familyId: string | null) => {
  dashboardStore.fetchUpcomingEvents(familyId || undefined);
};

onMounted(() => {
  fetchEvents(props.familyId);
});

watch(() => props.familyId, (newFamilyId) => {
  fetchEvents(newFamilyId);
});
</script>

<style scoped>
.scrollable-card-content {
  max-height: 400px; /* Adjust as needed */
  overflow-y: auto;
}
</style>