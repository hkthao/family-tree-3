import { ref } from 'vue';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';

interface UseMemberFaceDeletionOptions {
  deleteMutation: (id: string) => Promise<any>;
  successMessageKey: string;
  errorMessageKey: string;
  confirmationTitleKey: string;
  confirmationMessageKey: string;
  refetchList?: () => void;
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}

export function useMemberFaceDeletion(options: UseMemberFaceDeletionOptions) {
  const {
    deleteMutation,
    successMessageKey,
    errorMessageKey,
    confirmationTitleKey,
    confirmationMessageKey,
    refetchList,
    onSuccess,
    onError,
  } = options;

  const { t } = useI18n();
  const { showConfirmDialog } = useConfirmDialog();
  const { showSnackbar } = useGlobalSnackbar();

  const isDeleting = ref(false);

  const confirmAndDelete = async (memberFaceId: string, memberFaceDescription?: string) => {
    const confirmed = await showConfirmDialog({
      title: t(confirmationTitleKey),
      message: t(confirmationMessageKey, { description: memberFaceDescription || memberFaceId }),
      confirmText: t('common.delete'),
      cancelText: t('common.cancel'),
      confirmColor: 'error',
    });

    if (confirmed) {
      isDeleting.value = true;
      try {
        await deleteMutation(memberFaceId);
        showSnackbar(t(successMessageKey), 'success');
        onSuccess?.();
        refetchList?.();
      } catch (error: any) {
        showSnackbar(error.message || t(errorMessageKey), 'error');
        onError?.(error);
      } finally {
        isDeleting.value = false;
      }
    }
  };

  return {
    isDeleting,
    confirmAndDelete,
  };
}