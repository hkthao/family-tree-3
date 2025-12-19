import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMutation } from '@tanstack/vue-query';
import { useGlobalSnackbar } from '@/composables';
import { ApiFamilyService } from '@/services/family/api.family.service';
import apiClient from '@/plugins/axios';
import type { FamilyExportDto } from '@/types';

export function useFamilyDataManagement(familyId: string) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const apiFamilyService = new ApiFamilyService(apiClient);

  const importFile = ref<File | null>(null);
  const clearExistingData = ref(true);

  // Export Family Data Mutation
  const { mutateAsync: exportFamilyDataMutation, isPending: isExportingFamilyData } = useMutation<FamilyExportDto, Error, string>({
    mutationFn: async (id: string) => {
      const result = await apiFamilyService.exportFamilyData(id);
      if (result.ok) {
        return result.value;
      } else {
        throw new Error(result.error?.message || t('family.export.error'));
      }
    },
    onSuccess: (data: FamilyExportDto) => {
      showSnackbar(t('family.export.success'), 'success');
      // Convert FamilyExportDto to Blob and trigger file download
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `family-data-${familyId}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    },
    onError: (error) => {
      showSnackbar(`${t('family.export.error')}: ${error.message}`, 'error');
    },
  });

  // Import Family Data Mutation
  const { mutateAsync: importFamilyDataMutation, isPending: isImportingFamilyData } = useMutation<string, Error, { id: string; familyData: FamilyExportDto; clearExistingData: boolean }>({
    mutationFn: async ({ id, familyData, clearExistingData }) => {
      const result = await apiFamilyService.importFamilyData(id, familyData, clearExistingData);
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
      const reader = new FileReader();

      reader.onload = async (e) => {
        try {
          const fileContent = e.target?.result as string;
          const familyData: FamilyExportDto = JSON.parse(fileContent);
          await importFamilyDataMutation({ id: familyId, familyData, clearExistingData: clearExistingData.value });
        } catch (parseError) {
          console.error('Error parsing JSON file:', parseError);
          showSnackbar(t('family.import.parse_error'), 'error');
        }
      };

      reader.onerror = (e) => {
        console.error('Error reading file:', e);
        showSnackbar(t('family.import.read_error'), 'error');
      };

      reader.readAsText(file);
    } catch (error) {
      console.error('Error importing family data:', error);
      showSnackbar(t('family.import.error'), 'error');
    }
  };

  return {
    importFile,
    clearExistingData,
    exportData,
    isExportingFamilyData,
    importData,
    isImportingFamilyData,
  };
}
