import { ref, type Ref, watch } from 'vue'; // Removed onMounted
import { useProfileSettings } from '@/composables/user/useProfileSettings'; // Added useProfileSettings
import { NovuUI } from '@novu/js/ui';
import { dark } from '@novu/js/themes';
import { useI18n } from 'vue-i18n';
import { getEnvVariable } from '@/utils/api.util';

interface NovuOptions {
  options: {
    applicationIdentifier: string;
    subscriberId: string;
  };
}

export function useNovuInbox(containerRef: Ref<HTMLElement | null>) {
  const { t } = useI18n();
  const { userProfile, isFetchingProfile } = useProfileSettings();
  let novuInstance: NovuUI | null = null;

  const applicationIdentifier = ref(getEnvVariable('VITE_NOVU_APPLICATION_IDENTIFIER') || '');

  const initializeNovu = () => { // Changed to non-async function
    if (!containerRef.value) {
      console.error('Novu inbox container element not found');
      return;
    }

    if (!userProfile.value || isFetchingProfile.value) { // Check if profile is available and not fetching
      return;
    }

    const subscriberId = userProfile.value?.userId;

    if (!subscriberId) {
      console.error('Subscriber ID not found for Novu Inbox initialization.');
      return;
    }

    try {
      const novu = new NovuUI({
        options: {
          applicationIdentifier: applicationIdentifier.value,
          subscriberId: subscriberId,
        },
        container: containerRef.value,
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
            filter: { read: false },
          },
        ],
      } as NovuOptions);

      novu.mountComponent({
        name: 'InboxContent',
        props: {},
        element: containerRef.value,
      });

      novuInstance = novu;
    } catch (error) {
      console.error('Failed to initialize Novu Inbox:', error);
    }
  };



  watch([userProfile, isFetchingProfile, containerRef], () => { // Watch for userProfile to be available
    if (userProfile.value && !isFetchingProfile.value && containerRef.value) {
      initializeNovu();
    }
  }, { immediate: true });

  return {
    novuInstance,
    initializeNovu,
  };
}
