import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMutation } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyImportDto } from '@/types';
import { defaultFileDownloadAdapter, type IFileDownloadAdapter } from '@/composables/utils/fileDownload.adapter';
import { parseJsonFile } from '@/composables/utils/fileParser.utils';

interface UseFamilyDataManagementDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  fileDownloadAdapter: IFileDownloadAdapter;
  familyService: () => IFamilyService;
}

const defaultDeps: UseFamilyDataManagementDeps = {
  useI18n,
  useGlobalSnackbar,
  fileDownloadAdapter: defaultFileDownloadAdapter,
  familyService: () => useServices().family,
};

export function useFamilyDataManagement(familyId: string, deps: UseFamilyDataManagementDeps = defaultDeps) {
  const { useI18n, useGlobalSnackbar, familyService: getFamilyService } = deps;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const familyService = getFamilyService();

  const importFile = ref<File | null>(null);
  const clearExistingData = ref(true);

  // Import Family Data Mutation
  const { mutateAsync: importFamilyDataMutation, isPending: isImportingFamilyData } = useMutation<string, Error, { familyData: FamilyImportDto; clearExistingData: boolean }>({
    mutationFn: async ({ familyData, clearExistingData }) => {
      const result = await familyService.importFamilyData(familyData, clearExistingData);
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error?.message || t('family.import.error'));
      }
    },
    onSuccess: (newFamilyId: string) => {
      showSnackbar(`${t('family.import.success')}: ${newFamilyId}`, 'success');
      importFile.value = null; // Clear file input
    },
    onError: (error) => {
      showSnackbar(`${t('family.import.error')}: ${error.message}`, 'error');
    },
  });

  const importData = async () => {
    if (!importFile.value) return;

    try {
      const file = importFile.value;
      const familyData: FamilyImportDto = await parseJsonFile(file);
      await importFamilyDataMutation({ familyData, clearExistingData: clearExistingData.value });
    } catch (error: any) {
      console.error('Error importing family data:', error);
      showSnackbar(error.message || t('family.import.error'), 'error');
    }
  };

  return {
    state: {
      importFile,
      clearExistingData,
      isImportingFamilyData,
    },
    actions: {
      importData,
    },
  };
}