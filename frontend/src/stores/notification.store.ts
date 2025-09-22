import { defineStore } from 'pinia';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    snackbar: {
      show: false,
      message: '',
      color: 'success',
    },
  }),
  actions: {
    showSnackbar(message: string, color: string = 'success') {
      this.snackbar.show = true;
      this.snackbar.message = message;
      this.snackbar.color = color;
    },
    hideSnackbar() {
      this.snackbar.show = false;
    },
  },
});