<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12" md="4">
        <FamilyAutocomplete v-model="selectedFamilyId" label="Lọc theo gia đình" clearable hide-details
          prepend-inner-icon="mdi-filter-variant" />
      </v-col>
    </v-row>

    <!-- Top Summary Section -->
    <v-row>
      <v-col cols="12">
        <DashboardStats />
      </v-col>
    </v-row>

    <!-- Middle Section: Recent Activity & Upcoming Birthdays -->
    <v-row>
      <v-col cols="12" md="6">
        <RecentActivity />
      </v-col>
      <v-col cols="12" md="6">
        <UpcomingBirthdays />
      </v-col>
    </v-row>

    <!-- Bottom Section: Family Tree Overview & System Status -->
    <v-row>
      <v-col cols="12" md="6">
        <FamilyTreeOverview />
      </v-col>
      <v-col cols="12" md="6">
        <SystemStatus />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { useDashboardStore } from '@/stores/dashboard.store';
// import { useFamilyStore } from '@/stores/family.store'; // Removed
import DashboardStats from '@/components/dashboard/DashboardStats.vue';
import RecentActivity from '@/components/dashboard/RecentActivity.vue';
import UpcomingBirthdays from '@/components/dashboard/UpcomingBirthdays.vue';
import FamilyTreeOverview from '@/components/dashboard/FamilyTreeOverview.vue';
import SystemStatus from '@/components/dashboard/SystemStatus.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue'; // New import

const dashboardStore = useDashboardStore();
// const familyStore = useFamilyStore(); // Removed

const selectedFamilyId = ref<string | null>(null);

onMounted(async () => {
  // await familyStore.searchLookup({}, 1, 100); // Removed
  // Fetch all dashboard data when the component is mounted, initially without a filter
  dashboardStore.fetchAllDashboardData();
});

watch(selectedFamilyId, (newFamilyId) => {
  dashboardStore.fetchAllDashboardData(newFamilyId || undefined);
});
</script>

<style scoped>
/* Add any specific styles for the Dashboard page here */
</style>
