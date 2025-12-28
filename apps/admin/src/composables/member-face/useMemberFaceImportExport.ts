import { type Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { downloadFile } from '@/utils/file-helpers';
import { useServices } from '@/plugins/services.plugin';
import { useMutation } from '@tanstack/vue-query'; // Import useMutation

export function useMemberFaceImportExport(memberId: Ref<string | undefined>, familyId: Ref<string | undefined>) {
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const { memberFace } = useServices();

  const exportMemberFacesMutation = useMutation({
    mutationFn: async () => {
      console.log('exportMemberFaces mutationFn triggered');
      const result = await memberFace.exportMemberFaces(memberId.value, familyId.value);
      if (result.ok && result.value) {
        downloadFile(result.value, `member-faces-${memberId.value || familyId.value || 'all'}.json`, 'application/json');
        showSnackbar(t('memberFace.messages.exportSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('memberFace.messages.exportError'), 'error');
        throw new Error(result.error?.message || t('memberFace.messages.exportError')); // Throw error to be caught by onError
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('memberFace.messages.exportError'), 'error');
    },
  });

  const importMemberFacesMutation = useMutation({
    mutationFn: async (jsonData: any) => {
      const result = await memberFace.importMemberFaces(memberId.value, familyId.value, jsonData);
      if (result.ok) {
        showSnackbar(t('memberFace.messages.importSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('memberFace.messages.importError'), 'error');
        throw new Error(result.error?.message || t('memberFace.messages.importError')); // Throw error to be caught by onError
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('memberFace.messages.importError'), 'error');
    },
  });

  return {
    isExporting: exportMemberFacesMutation.isPending,
    exportMemberFaces: exportMemberFacesMutation.mutateAsync,
    isImporting: importMemberFacesMutation.isPending,
    importMemberFaces: importMemberFacesMutation.mutateAsync,
  };
}
