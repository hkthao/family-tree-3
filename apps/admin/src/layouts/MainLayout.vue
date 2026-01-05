<script setup lang="ts">

import { ref, onMounted, computed } from 'vue';
import { Sidebar, TopBar } from '@/components/layout';
import AppBreadcrumbs from '@/components/common/AppBreadcrumbs.vue';
import GlobalSnackbar from '@/components/common/GlobalSnackbar.vue';
import DonateMessage from '@/components/common/DonateMessage.vue';
import ConfirmDialog from '@/components/common/ConfirmDialog.vue';
import MapLocationDrawer from '@/components/common/MapLocationDrawer.vue';
import MediaPickerDrawer from '@/components/family-media/MediaPickerDrawer.vue'; // NEW

import { useAuthStore } from '@/stores';

const drawer = ref(true);
const authStore = useAuthStore();
const currentUser = computed(() => authStore.user);

onMounted(async () => {
  await authStore.initAuth();
});

</script>

<template>
  <v-app>
    <MapLocationDrawer />
    <Sidebar v-model="drawer" :current-user="currentUser" />
    <TopBar @toggle-drawer="drawer = !drawer" :current-user="currentUser" />
    <v-main>
      <AppBreadcrumbs />
      <DonateMessage />
      <router-view />
    </v-main>
    <GlobalSnackbar />
    <ConfirmDialog />
    <MediaPickerDrawer />
  </v-app>
</template>
