<template>
  <v-container fluid>
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
import { onMounted } from 'vue';
import { useDashboardStore } from '@/stores/dashboard.store';
import DashboardStats from '@/components/dashboard/DashboardStats.vue';
import RecentActivity from '@/components/dashboard/RecentActivity.vue';
import UpcomingBirthdays from '@/components/dashboard/UpcomingBirthdays.vue';
import FamilyTreeOverview from '@/components/dashboard/FamilyTreeOverview.vue';
import SystemStatus from '@/components/dashboard/SystemStatus.vue'; // Renamed import

const dashboardStore = useDashboardStore();

onMounted(() => {
  // Fetch all dashboard data when the component is mounted
  dashboardStore.fetchAllDashboardData();
});
</script>

<style scoped>
/* Add any specific styles for the Dashboard page here */
</style>
