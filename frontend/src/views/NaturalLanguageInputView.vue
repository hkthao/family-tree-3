<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <PromptInput />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <ResultDisplay />
      </v-col>
    </v-row>
    <v-snackbar
      v-model="notificationStore.snackbar.show"
      :color="notificationStore.snackbar.color"
      :timeout="notificationStore.snackbar.timeout"
      bottom
      right
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import PromptInput from '@/components/NaturalLanguageInput/PromptInput.vue';
import ResultDisplay from '@/components/NaturalLanguageInput/ResultDisplay.vue';
import { useNotificationStore } from '@/stores/notification.store';

const notificationStore = useNotificationStore();
</script>

<style scoped>
/* Add any specific styles here if needed */
</style>