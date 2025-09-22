import { defineStore } from 'pinia';

interface SnackbarState {
  show: boolean;
  message: string;
  color: string;
}

interface NotificationState {
  snackbar: SnackbarState;
}

export const useNotificationStore = defineStore('notification', {
  state: (): NotificationState => ({
    snackbar: {
      show: false,
      message: '',
      color: 'success',
    },
  }),

  actions: {
    showSnackbar(message: string, color: string = 'success') {
      this.snackbar.message = message;
      this.snackbar.color = color;
      this.snackbar.show = true;
    },

    hideSnackbar() {
      this.snackbar.show = false;
    },
  },
});
