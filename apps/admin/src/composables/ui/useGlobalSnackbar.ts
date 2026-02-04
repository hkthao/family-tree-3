import { useNotificationStore } from '@/stores/notification.store';

export function useGlobalSnackbar() {
  const notificationStore = useNotificationStore();

  const showSnackbar = (message: string, color: string = 'info', timeout: number = 3000) => {
    notificationStore.showSnackbar(message, color, timeout);
  };

  const showSuccess = (message: string, timeout: number = 3000) => {
    showSnackbar(message, 'success', timeout);
  };

  const showError = (message: string, timeout: number = 3000) => {
    showSnackbar(message, 'error', timeout);
  };

  return {
    showSnackbar,
    showSuccess,
    showError,
  };
}

export type UseGlobalSnackbarReturn = ReturnType<typeof useGlobalSnackbar>;
