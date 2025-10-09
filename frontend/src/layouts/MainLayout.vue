<template>
  <v-app>
    <Sidebar v-model="drawer" :current-user="currentUser" />
    <TopBar @toggle-drawer="drawer = !drawer" :current-user="currentUser" />
    <v-main>
      <AppBreadcrumbs />
      <router-view />
    </v-main>

    <v-snackbar
      v-model="notificationStore.snackbar.show"
      :timeout="notificationStore.snackbar.timeout"
      :color="notificationStore.snackbar.color"
      location="bottom center"
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';
import AppBreadcrumbs from '@/components/common/AppBreadcrumbs.vue';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationStore } from '@/stores/notification.store';

const drawer = ref(true);
const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const currentUser = computed(() => authStore.user);

onMounted(async () => {
  await authStore.initAuth();
});

watch(() => notificationStore.snackbar.show, (newVal) => {
  if (!newVal) {
    notificationStore.resetNotification();
  }
});
</script>
