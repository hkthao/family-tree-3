<template>
  <v-container fluid>
    <v-row >
      <v-col cols="12" md="4">
        <FamilyAutocomplete
          v-model="selectedFamilyId"
          label="Lọc theo gia đình"
          clearable
          hide-details
          prepend-inner-icon="mdi-filter-variant"
        />
      </v-col>
    </v-row>

    <!-- Top Summary Section -->
    <v-row>
      <v-col cols="12">
        <DashboardStats />
      </v-col>
    </v-row>

    <!-- Middle Section: Recent Activity & Upcoming Events -->
    <v-row>
      <v-col cols="12" md="6">
        <RecentActivity />
      </v-col>
      <v-col cols="12" md="6">
        <UpcomingEvents />
      </v-col>
    </v-row>

    <!-- Bottom Section: Family Tree Overview -->
    <v-row>
      <v-col cols="12">
        <FamilyTreeOverview :family-id="selectedFamilyId" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useDashboardStore } from '@/stores/dashboard.store';
import DashboardStats from '@/components/dashboard/DashboardStats.vue';
import RecentActivity from '@/components/dashboard/RecentActivity.vue';
import UpcomingEvents from '@/components/dashboard/UpcomingEvents.vue'; // Changed import
import FamilyTreeOverview from '@/components/dashboard/FamilyTreeOverview.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const dashboardStore = useDashboardStore();

const selectedFamilyId = ref<string | null>(null);

onMounted(async () => {
  dashboardStore.fetchAllDashboardData();
});

watch(selectedFamilyId, (newFamilyId) => {
  dashboardStore.fetchAllDashboardData(newFamilyId || undefined);
});
</script>

<style scoped>
/* Add any specific styles for the Dashboard page here */
</style>