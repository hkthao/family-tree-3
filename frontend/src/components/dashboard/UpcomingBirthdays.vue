<template>
  <v-card class="pa-4" elevation="2">
    <v-card-title class="d-flex align-center">
      <v-icon left>mdi-cake-variant</v-icon>
      <span class="ml-2">Sinh nhật sắp tới</span>
    </v-card-title>
    <v-card-text>
      <div v-if="dashboardStore.loading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        <p class="mt-2">Đang tải sinh nhật sắp tới...</p>
      </div>
      <v-alert v-else-if="dashboardStore.error" type="error" dense dismissible class="mb-4">
        {{ dashboardStore.error }}
      </v-alert>
      <v-list v-else dense>
        <v-list-item v-for="item in dashboardStore.dashboardData.upcomingBirthdays" :key="item.id">
          <template v-slot:prepend>
            <v-avatar :image="item.avatar || 'https://cdn.vuetifyjs.com/images/cards/halcyon.png'"></v-avatar>
          </template>
          <v-list-item-title>{{ item.name }}</v-list-item-title>
          <v-list-item-subtitle>
            {{ new Date(item.dateOfBirth).toLocaleDateString() }}
            <v-chip v-if="item.age" class="ml-2" color="primary" size="small">{{ item.age }} tuổi</v-chip>
          </v-list-item-subtitle>
        </v-list-item>
        <v-list-item v-if="dashboardStore.dashboardData.upcomingBirthdays.length === 0">
          <v-list-item-title>Không có sinh nhật sắp tới.</v-list-item-title>
        </v-list-item>
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useDashboardStore } from '@/stores/dashboard.store';
import { onMounted } from 'vue';

const dashboardStore = useDashboardStore();

onMounted(() => {
  dashboardStore.fetchUpcomingBirthdays();
});
</script>

<style scoped>
/* Add any specific styles for UpcomingBirthdays here */
</style>