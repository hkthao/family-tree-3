import { ref, type Ref, watch, onUnmounted } from 'vue'; // Added onUnmounted
import { useUserProfile } from '@/composables'; // Changed useProfileSettings to useUserProfile
import { NovuUI } from '@novu/js/ui';
import { dark } from '@novu/js/themes';
import { useI18n } from 'vue-i18n';
import { getEnvVariable } from '@/utils/api.util';

export function useNovuInbox(containerRef: Ref<HTMLElement | null>) {
  const { t } = useI18n();
  const { state: { userProfile } } = useUserProfile(); // Use useUserProfile
  let novuInstance: NovuUI | null = null;

  const applicationIdentifier = ref(getEnvVariable('VITE_NOVU_APPLICATION_IDENTIFIER') || '');

  const destroyNovuInstance = () => {
    if (novuInstance) {
      // novuInstance.unmountComponent(); // NovuUI might not have a direct unmount for the whole instance, nullifying is sufficient
      novuInstance = null;
    }
  };

  watch([() => userProfile.value?.userId, containerRef], ([newSubscriberId, newContainerRef]) => {
    // If subscriberId or containerRef changes, destroy existing instance
    destroyNovuInstance();

    if (!newSubscriberId) {
      console.warn('Subscriber ID not found for Novu Inbox initialization.');
      return;
    }

    if (!newContainerRef) {
      console.warn('Novu inbox container element not found. Retrying when container is available.');
      return;
    }

    // Initialize only if a valid subscriberId and container are present and no instance exists
    if (newSubscriberId && newContainerRef && !novuInstance) {
      try {
        novuInstance = new NovuUI({
          options: {
            applicationIdentifier: applicationIdentifier.value,
            subscriberId: newSubscriberId,
          },
          container: newContainerRef,
          appearance: {
            baseTheme: dark,
          },
          tabs: [
                      {
                        label: t('novu.tabs.all'),
                        filter: {},
                      },
                      {
                        label: t('novu.tabs.unread'),
                        filter: {}, // No direct filter for 'read' status in NovuUI tabs configuration
                      },          ],
        });

        novuInstance.mountComponent({
          name: 'InboxContent',
          props: {},
          element: newContainerRef,
        });

      } catch (error) {
        console.error('Failed to initialize Novu Inbox:', error);
      }
    }
  }, { immediate: true });



  onUnmounted(() => {
    destroyNovuInstance();
  });

  return {
    novuInstance,
  };
}
