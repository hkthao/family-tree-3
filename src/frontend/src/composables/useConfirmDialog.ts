import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  color?: string;
  width?: string; // New
  cancelColor?: string; // New
  confirmColor?: string; // New
}

const isDialogOpen = ref(false);
const dialogOptions = ref<ConfirmDialogOptions>({
  title: '',
  message: '',
  confirmText: '',
  cancelText: '',
  color: 'primary',
});
let resolvePromise: ((confirm: boolean) => void) | null = null;

export function useConfirmDialog() {
  const { t } = useI18n();

  const showConfirmDialog = (options: ConfirmDialogOptions): Promise<boolean> => {
    dialogOptions.value = {
      title: options.title,
      message: String(options.message), // Explicitly cast to string
      confirmText: options.confirmText || t('common.confirm'),
      cancelText: options.cancelText || t('common.cancel'),
      color: options.color || 'primary',
      width: options.width,
      cancelColor: options.cancelColor,
      confirmColor: options.confirmColor,
    };
    isDialogOpen.value = true;

    return new Promise<boolean>((resolve) => {
      resolvePromise = resolve;
    });
  };

  const confirm = () => {
    isDialogOpen.value = false;
    if (resolvePromise) {
      resolvePromise(true);
    }
  };

  const cancel = () => {
    isDialogOpen.value = false;
    if (resolvePromise) {
      resolvePromise(false);
    }
  };

  return {
    isDialogOpen,
    dialogOptions,
    showConfirmDialog,
    confirm,
    cancel,
  };
}
