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
import { ref, onMounted, onUnmounted } from 'vue';
import { useUserProfileStore } from '@/stores';
import { Novu } from '@novu/js';
import NovuInbox from './NovuInbox.vue'

const userProfileStore = useUserProfileStore();
const unseenCount = ref(0);
let novuDispose: (() => void) | null = null;
const applicationIdentifier = ref(import.meta.env.VITE_NOVU_APPLICATION_IDENTIFIER || '');

const updateUnReadCount = async (novu: Novu) => {
  const { data } = await novu.notifications.count({
    read: false
  });
  console.log("updateCount", data);
  if (data)
    unseenCount.value = data.count;
  else
    unseenCount.value = 0
}

onMounted(async () => {
  await userProfileStore.fetchCurrentUserProfile();
  const subscriberId = userProfileStore.userProfile?.id;

  if (!subscriberId) {
    console.warn('Subscriber ID not found. Novu notifications will not be initialized.');
    return;
  }

  try {
    const novu = new Novu({
      applicationIdentifier: applicationIdentifier.value,
      subscriberId: subscriberId,
    });

    // Lấy số count ban đầu
    await updateUnReadCount(novu)

    novuDispose = novu.on('notifications.unseen_count_changed', async (data) => {
      console.log('notifications.unseen_count_changed', data);
      await updateUnReadCount(novu)
    });

    novu.on('notifications.notification_received', async (data) => {
      console.log('notifications.notification_received', data);
      await updateUnReadCount(novu)
    });

  } catch (error) {
    console.error('Failed to initialize Novu client:', error);
  }
});

onUnmounted(() => {
  if (novuDispose) {
    novuDispose();
  }
});
</script>

<style>
.inbox-contaner {
  overflow-x: hidden !important;
}
</style>