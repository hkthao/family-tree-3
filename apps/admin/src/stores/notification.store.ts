import { defineStore } from 'pinia';

interface SnackbarState {
  show: boolean;
  message: string;
  color: string;
  timeout: number;
}

export const useNotificationStore = defineStore('notification', {
  state: (): { snackbar: SnackbarState } => ({
    snackbar: {
      show: false,
      message: '',
      color: '',
      timeout: 3000,
    },
  }),

  actions: {
    showSnackbar(message: string, color = 'success', timeout = 3000) {
      this.snackbar.show = true;
      this.snackbar.message = message;
      this.snackbar.color = color;
      this.snackbar.timeout = timeout;
    },
    resetSnackbar() {
      this.snackbar.show = false;
      this.snackbar.message = '';
      this.snackbar.color = '';
      this.snackbar.timeout = 3000;
    },
  },
});
