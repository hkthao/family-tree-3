import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useNotificationStore = defineStore('notification', () => {
  const snackbar = ref({
    show: false,
    message: '',
    color: '',
  });

  function showSnackbar(message: string, color: string = 'info') {
    snackbar.value = {
      show: true,
      message,
      color,
    };
  }

  function hideSnackbar() {
    snackbar.value.show = false;
  }

  return {
    snackbar,
    showSnackbar,
    hideSnackbar,
  };
});
