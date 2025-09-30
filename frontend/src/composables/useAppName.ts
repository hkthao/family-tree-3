import { ref } from 'vue';

const appName = ref('Cây Gia Phả'); // Centralized app name

export function useAppName() {
  return { appName };
}
