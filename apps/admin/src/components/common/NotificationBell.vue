<template>
  <v-menu :close-on-content-click="false">
    <template v-slot:activator="{ props }">
      <v-btn v-bind="props" icon>
        <v-badge :content="unseenCount" color="error" :model-value="unseenCount > 0">
          <v-icon>mdi-bell-outline</v-icon>
        </v-badge>
      </v-btn>
    </template>
    <v-sheet class="inbox-contaner">
      <NovuInbox />
    </v-sheet>
  </v-menu>
</template>

<script setup lang="ts">
import NovuInbox from './NovuInbox.vue';
import { useNovuNotificationBell } from '@/composables';

const { unseenCount } = useNovuNotificationBell();
</script>

<style>
.inbox-contaner {
  overflow-x: hidden !important;
  max-width: 400px;
}
</style>