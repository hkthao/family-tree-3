<template>
  <v-btn icon>
    <v-badge :content="unseenCount" color="error" :model-value="unseenCount > 0">
      <v-icon>mdi-bell-outline</v-icon>
    </v-badge>
  </v-btn>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useUserProfileStore } from '@/stores';
import { Novu } from '@novu/js';

const userProfileStore = useUserProfileStore();
const unseenCount = ref(0);
let novuDispose: (() => void) | null = null;

// Placeholder for applicationIdentifier - needs to be configured
const applicationIdentifier = ref(import.meta.env.VITE_NOVU_APPLICATION_IDENTIFIER || '');

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
    const { data } = await novu.notifications.count();
    if (data)
      unseenCount.value = data.count;

    novuDispose = novu.on('notifications.unseen_count_changed', (data) => {
      console.log('notifications.unseen_count_changed');
      unseenCount.value = data.result;
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