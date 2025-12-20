import { useNotificationStore } from '@/stores/notification.store';

export function useGlobalSnackbar() {
  const notificationStore = useNotificationStore();

  const showSnackbar = (message: string, color: string = 'info', timeout: number = 3000) => {
    notificationStore.showSnackbar(message, color, timeout);
  };

  return {
    showSnackbar,
  };
}

export type UseGlobalSnackbarReturn = ReturnType<typeof useGlobalSnackbar>;
