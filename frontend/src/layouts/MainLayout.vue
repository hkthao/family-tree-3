<template>
  <v-app>
    <Sidebar v-model="drawer" :current-user="currentUser" />
    <TopBar @toggle-drawer="drawer = !drawer" :current-user="currentUser" />
    <v-main>
      <AppBreadcrumbs />
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';
import AppBreadcrumbs from '@/components/common/AppBreadcrumbs.vue';
import { useAuthStore } from '@/stores/auth.store';

const drawer = ref(true);
const authStore = useAuthStore();
const currentUser = computed(() => authStore.user);

onMounted(async () => {
  await authStore.initAuth();
});</script>
