import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMutation } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import type { IFamilyService } from '@/services/family/family.service.interface';
import { useServices } from '@/composables';
import type { FamilyExportDto } from '@/types';
import { defaultFileDownloadAdapter, type IFileDownloadAdapter } from '@/composables/utils/fileDownload.adapter';
import { parseJsonFile } from '@/composables/utils/fileParser.utils';

interface UseFamilyDataManagementDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  fileDownloadAdapter: IFileDownloadAdapter;
  familyService: IFamilyService;
}

const defaultDeps: UseFamilyDataManagementDeps = {
  useI18n,
  useGlobalSnackbar,
  fileDownloadAdapter: defaultFileDownloadAdapter,
  familyService: useServices().family,
};

export function useFamilyDataManagement(familyId: string, deps: UseFamilyDataManagementDeps = defaultDeps) {
  const { useI18n, useGlobalSnackbar, fileDownloadAdapter, familyService } = deps;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();



  const importFile = ref<File | null>(null);
  const clearExistingData = ref(true);

  // Export Family Data Mutation
  const { mutateAsync: exportFamilyDataMutation, isPending: isExportingFamilyData } = useMutation<FamilyExportDto, Error, string>({
    mutationFn: async (id: string) => {
      const result = await familyService.exportFamilyData(id);
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error?.message || t('family.export.error'));
      }
    },
    onSuccess: (data: FamilyExportDto) => {
      showSnackbar(t('family.export.success'), 'success');
      fileDownloadAdapter.downloadJson(data, `family-data-${familyId}.json`);
    },
    onError: (error) => {
      showSnackbar(`${t('family.export.error')}: ${error.message}`, 'error');
    },
  });

  // Import Family Data Mutation
  const { mutateAsync: importFamilyDataMutation, isPending: isImportingFamilyData } = useMutation<string, Error, { id: string; familyData: FamilyExportDto; clearExistingData: boolean }>({
    mutationFn: async ({ id, familyData, clearExistingData }) => {
      const result = await familyService.importFamilyData(id, familyData, clearExistingData);
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

  const exportData = (id: string) => exportFamilyDataMutation(id);

  const importData = async () => {
    if (!importFile.value) return;

    try {
      const file = importFile.value;
      const familyData: FamilyExportDto = await parseJsonFile(file);
      await importFamilyDataMutation({ id: familyId, familyData, clearExistingData: clearExistingData.value });
    } catch (error: any) {
      console.error('Error importing family data:', error);
      showSnackbar(error.message || t('family.import.error'), 'error');
    }
  };

  return {
    state: {
      importFile,
      clearExistingData,
      isExportingFamilyData,
      isImportingFamilyData,
    },
    actions: {
      exportData,
      importData,
    },
  };
}
