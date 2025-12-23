import { ref, onUnmounted, watch } from 'vue';
import { useUserProfile } from '@/composables';
import { Novu } from '@novu/js';
import { getEnvVariable } from '@/utils/api.util';

export function useNovuNotificationBell() {
  const { state: { userProfile } } = useUserProfile();
  const unseenCount = ref(0);
  let novuClient: Novu | null = null;
  let unseen_count_changed_off: (() => void) | null = null; // Renamed for clarity
  let notification_received_off: (() => void) | null = null; // Renamed for clarity
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

  const destroyNovuClient = () => {
    if (unseen_count_changed_off) {
      unseen_count_changed_off();
      unseen_count_changed_off = null;
    }
    if (notification_received_off) {
      notification_received_off();
      notification_received_off = null;
    }
    novuClient = null;
  };

  watch(() => userProfile.value?.userId, async (newSubscriberId, oldSubscriberId) => {
    // Destroy existing client if subscriberId changes or becomes null
    if (newSubscriberId !== oldSubscriberId && novuClient) {
      destroyNovuClient();
    }

    if (!newSubscriberId) {
      console.warn('Subscriber ID not found. Novu notifications will not be initialized.');
      return;
    }

    // Only (re)initialize if a valid subscriberId is present and client is not already initialized for this ID
    if (newSubscriberId && !novuClient) {
      try {
        novuClient = new Novu({
          applicationIdentifier: applicationIdentifier.value,
          subscriberId: newSubscriberId,
        });

        // Fetch initial unread count
        await updateUnReadCount();

        // Set up event listeners
        unseen_count_changed_off = novuClient.on('notifications.unseen_count_changed', async () => {
          await updateUnReadCount();
        });

        notification_received_off = novuClient.on('notifications.notification_received', async () => {
          await updateUnReadCount();
        });

      } catch (error) {
        console.error('Failed to initialize Novu client:', error);
      }
    }
  }, { immediate: true }); // Run immediately to catch initial userProfile state

  onUnmounted(() => {
    destroyNovuClient();
  });

  return {
    unseenCount,
  };
}

