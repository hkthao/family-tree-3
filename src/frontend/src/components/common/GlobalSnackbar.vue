<template>
  <v-snackbar v-model="snackbar.show" :color="snackbar.color" timeout="3000">
    {{ snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { useNotificationStore } from '@/stores/notification.store';
import { storeToRefs } from 'pinia';

const notificationStore = useNotificationStore();
const { snackbar } = storeToRefs(notificationStore);
</script>
