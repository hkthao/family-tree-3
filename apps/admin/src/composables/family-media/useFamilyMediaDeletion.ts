import { ref, unref } from 'vue';
import type { Ref } from 'vue';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';

interface UseFamilyMediaDeletionOptions {
  familyId: Ref<string>;
  deleteMutation: (data: { familyId: string; id: string }) => Promise<any>;
  successMessageKey: string;
  errorMessageKey: string;
  confirmationTitleKey: string;
  confirmationMessageKey: string;
  refetchList?: () => void;
  onSuccess?: () => void;
  onError?: (error: Error) => void;
}

export function useFamilyMediaDeletion(options: UseFamilyMediaDeletionOptions) {
  const {
    familyId,
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

  const confirmAndDelete = async (mediaId: string, mediaName: string) => {
    if (!unref(familyId)) {
      showSnackbar(t('familyMedia.errors.familyIdRequired'), 'error');
      return;
    }

    const confirmed = await showConfirmDialog({
      title: t(confirmationTitleKey),
      message: t(confirmationMessageKey, { fileName: mediaName }),
      confirmText: t('common.delete'),
      cancelText: t('common.cancel'),
      confirmColor: 'error',
    });

    if (confirmed) {
      isDeleting.value = true;
      try {
        await deleteMutation({ familyId: unref(familyId), id: mediaId });
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