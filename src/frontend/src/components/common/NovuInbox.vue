<template>
  <div ref="novuInbox" class="novu-inbox-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useUserProfileStore } from '@/stores';
import { NovuUI } from '@novu/js/ui';
import { dark } from '@novu/js/themes'; // Import the dark theme object

interface NovuOptions {
  options: {
    applicationIdentifier: string;
    subscriberId: string;
  };
}

const userProfileStore = useUserProfileStore();
const novuInbox = ref<HTMLElement | null>(null);
let novuInstance: NovuUI | null = null;

// Placeholder for applicationIdentifier - needs to be configured
const applicationIdentifier = ref(import.meta.env.VITE_NOVU_APPLICATION_IDENTIFIER || '');

// Get subscriberId from the authenticated user

onMounted(async () => {
  if (!novuInbox.value) {
    console.error('Novu inbox container element not found');
    return;
  }
  await userProfileStore.fetchCurrentUserProfile()
  const subscriberId = userProfileStore.userProfile?.id
  if (!subscriberId)
    return

  try {
    const novu = new NovuUI({
      options: {
        applicationIdentifier: applicationIdentifier.value,
        subscriberId: subscriberId,
      },
      container: novuInbox.value,
      appearance: {
        baseTheme: dark, // Use the imported dark theme object
      },
    } as NovuOptions);

    // Mount the Inbox component to our div reference
    // This is where Novu creates and injects its Inbox UI
    novu.mountComponent({
      name: "InboxContent",
      props: {
      },
      element: novuInbox.value, // The actual DOM element where Inbox will be mounted
    });

    // Store the instance for cleanup
    novuInstance = novu;

  } catch (error) {
    console.error('Failed to initialize Novu Inbox:', error);
  }
});

onUnmounted(() => {
  if (novuInstance && novuInbox.value) {
    try {
      // Properly unmount the Novu component to prevent memory leaks
      novuInstance.unmountComponent(novuInbox.value);
    } catch (error) {
      console.error("Failed to unmount Novu inbox:", error);
    }
  }
});
</script>
