import { ref, onMounted, onUnmounted, type Ref } from 'vue';
import { useUserProfileStore } from '@/stores';
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
  const userProfileStore = useUserProfileStore();
  let novuInstance: NovuUI | null = null;

  const applicationIdentifier = ref(getEnvVariable('VITE_NOVU_APPLICATION_IDENTIFIER') || '');

  const initializeNovu = async () => {
    if (!containerRef.value) {
      console.error('Novu inbox container element not found');
      return;
    }

    await userProfileStore.fetchCurrentUserProfile();
    const subscriberId = userProfileStore.userProfile?.userId;

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

  const cleanupNovu = () => {
    if (novuInstance && containerRef.value) {
      try {
        novuInstance.unmountComponent(containerRef.value);
      } catch (error) {
        console.error('Failed to unmount Novu inbox:', error);
      }
    }
  };

  onMounted(initializeNovu);
  onUnmounted(cleanupNovu);

  return {
    novuInstance,
    initializeNovu,
  };
}
