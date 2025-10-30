<template>
  <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000" :data-testid="snackbar.color === 'success' ? 'snackbar-success' : undefined">
    {{ snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { useNotificationStore } from '@/stores/notification.store';
import { storeToRefs } from 'pinia';

const notificationStore = useNotificationStore();
const { snackbar } = storeToRefs(notificationStore);
</script>
