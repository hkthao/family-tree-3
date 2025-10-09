import { defineStore } from 'pinia';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    snackbar: {
      show: false,
      message: '',
      color: 'success',
      timeout: 3000, // Default timeout
    },
  }),
  actions: {
    showSnackbar(message: string, color: string = 'success', timeout: number = 3000) {
      this.snackbar.show = true;
      this.snackbar.message = message;
      this.snackbar.color = color;
      this.snackbar.timeout = timeout;
    },
    hideSnackbar() {
      this.snackbar.show = false;
    },
    resetNotification() {
      this.snackbar.show = false;
      this.snackbar.message = '';
      this.snackbar.color = 'success';
      this.snackbar.timeout = 3000;
    },
  },
});