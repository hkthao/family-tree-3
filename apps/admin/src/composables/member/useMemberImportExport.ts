import { type Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import { downloadFile } from '@/utils/file-helpers';
import { useServices } from '@/plugins/services.plugin';
import { useMutation } from '@tanstack/vue-query';

export function useMemberImportExport(familyId: Ref<string | undefined>) {
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();
  const { member: memberService } = useServices(); // Renamed to avoid conflict with member variable

  const exportMembersMutation = useMutation({
    mutationFn: async () => {
      const result = await memberService.exportMembers(familyId.value);
      if (result.ok && result.value) {
        downloadFile(JSON.stringify(result.value), `members-${familyId.value || 'all'}.json`, 'application/json');
        showSnackbar(t('member.messages.exportSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('member.messages.exportError'), 'error');
        throw new Error(result.error?.message || t('member.messages.exportError'));
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('member.messages.exportError'), 'error');
    },
  });

  const importMembersMutation = useMutation({
    mutationFn: async (jsonData: any) => {
      if (!familyId.value) {
        showSnackbar(t('member.messages.noFamilyId'), 'error');
        throw new Error(t('member.messages.noFamilyId'));
      }
      const result = await memberService.importMembers(familyId.value, jsonData);
      if (result.ok) {
        showSnackbar(t('member.messages.importSuccess'), 'success');
      } else if (!result.ok) {
        showSnackbar(result.error?.message || t('member.messages.importError'), 'error');
        throw new Error(result.error?.message || t('member.messages.importError'));
      }
    },
    onError: (error) => {
      showSnackbar(error.message || t('member.messages.importError'), 'error');
    },
  });

  return {
    isExporting: exportMembersMutation.isPending,
    exportMembers: exportMembersMutation.mutateAsync,
    isImporting: importMembersMutation.isPending,
    importMembers: importMembersMutation.mutateAsync,
  };
}
