import { ref, onMounted, onUnmounted } from 'vue';
import { useProfileSettings } from '@/composables';
import { Novu } from '@novu/js';
import { getEnvVariable } from '@/utils/api.util';

export function useNovuNotificationBell() {
  const { userProfile } = useProfileSettings();
  const unseenCount = ref(0);
  let novuClient: Novu | null = null;
  let unseen_count_changed: (() => void) | null = null;
  let notification_received: (() => void) | null = null;
  const applicationIdentifier = ref(getEnvVariable('VITE_NOVU_APPLICATION_IDENTIFIER') || '');

  const updateUnReadCount = async () => {
    if (!novuClient) return;
    try {
      const { data } = await novuClient.notifications.count({
        read: false
      });
      unseenCount.value = data ? data.count : 0;
    } catch (error) {
      console.error('Failed to fetch unseen notification count:', error);
      unseenCount.value = 0;
    }
  };

  onMounted(async () => {
    const subscriberId = userProfile.value?.userId;

    if (!subscriberId) {
      console.warn('Subscriber ID not found. Novu notifications will not be initialized.');
      return;
    }

    try {
      novuClient = new Novu({
        applicationIdentifier: applicationIdentifier.value,
        subscriberId: subscriberId,
      });

      // Fetch initial unread count
      await updateUnReadCount();

      // Set up event listeners
      unseen_count_changed = novuClient.on('notifications.unseen_count_changed', async () => {
        await updateUnReadCount();
      });

      notification_received = novuClient.on('notifications.notification_received', async () => {
        await updateUnReadCount();
      });

    } catch (error) {
      console.error('Failed to initialize Novu client:', error);
    }
  });

  onUnmounted(() => {
    if (unseen_count_changed) {
      unseen_count_changed();
    }
    if (notification_received) {
      notification_received();
    }
    novuClient = null; // Clear client on unmount
  });

  return {
    unseenCount,
  };
}
