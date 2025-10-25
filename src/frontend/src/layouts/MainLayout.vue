<template>
  <v-app>
    <Sidebar v-model="drawer" :current-user="currentUser" />
    <TopBar @toggle-drawer="drawer = !drawer" :current-user="currentUser" />
    <v-main>
      <AppBreadcrumbs />
      <router-view />
    </v-main>

    <ChatWidget />

    <GlobalSnackbar />
  </v-app>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';
import AppBreadcrumbs from '@/components/common/AppBreadcrumbs.vue';
import ChatWidget from '@/components/ChatWidget.vue';
import GlobalSnackbar from '@/components/common/GlobalSnackbar.vue';
import { useAuthStore, useNotificationStore } from '@/stores';

const drawer = ref(true);
const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const currentUser = computed(() => authStore.user);

onMounted(async () => {
  await authStore.initAuth();
});


</script>
