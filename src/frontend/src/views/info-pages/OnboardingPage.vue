<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('onboarding.title') }}</h1>
        <p class="text-body-1 mb-6">
          {{ $t('onboarding.description') }}
        </p>
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('onboarding.interactiveTour.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('onboarding.interactiveTour.dashboardMessage') }}
            </p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// Trang này hiện chỉ hiển thị thông tin, tour đã được chuyển sang composable
// và có thể được kích hoạt từ Dashboard.
</script>

<style scoped>
/* Add any specific styles for the Onboarding page here */
</style>