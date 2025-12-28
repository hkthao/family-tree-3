import { ref, computed } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import type { FamilyLocationDto } from '@/types/familyLocation';
import type { ApiError } from '@/types/api';

export function useFamilyLocationImportExport(familyId: string) {
  const { t } = useI18n();
  const { familyLocation: familyLocationService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();

  const isExporting = ref(false);
  const isImporting = ref(false);
  const exportError = ref<ApiError | null>(null);
  const importError = ref<ApiError | null>(null);

  const exportFamilyLocations = async () => {
    isExporting.value = true;
    exportError.value = null;
    try {
      const result = await familyLocationService.exportFamilyLocations(familyId);
      if (result.ok) {
        const locationsToExport = result.value;
        const jsonString = JSON.stringify(locationsToExport, null, 2);
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `family-locations-export-${familyId}-${new Date().toISOString().slice(0, 10)}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        showSnackbar(t('familyLocation.messages.exportSuccess'), 'success');
      } else {
        exportError.value = result.error;
        showSnackbar(result.error.message || t('familyLocation.messages.exportError'), 'error');
      }
    } catch (err: any) {
      exportError.value = err;
      showSnackbar(err.message || t('familyLocation.messages.exportError'), 'error');
    } finally {
      isExporting.value = false;
    }
  };

  const importFamilyLocations = async (locations: any[]) => { // Use any for now, will map to DTO later
    isImporting.value = true;
    importError.value = null;
    try {
      const result = await familyLocationService.importFamilyLocations(familyId, locations);
      if (result.ok) {
        showSnackbar(t('familyLocation.messages.importSuccess', { count: result.value.length }), 'success');
        return true;
      } else {
        importError.value = result.error;
        showSnackbar(result.error.message || t('familyLocation.messages.importError'), 'error');
        return false;
      }
    } catch (err: any) {
      importError.value = err;
      showSnackbar(err.message || t('familyLocation.messages.importError'), 'error');
      return false;
    } finally {
      isImporting.value = false;
    }
  };

  return {
    isExporting: computed(() => isExporting.value),
    isImporting: computed(() => isImporting.value),
    exportError: computed(() => exportError.value),
    importError: computed(() => importError.value),
    exportFamilyLocations,
    importFamilyLocations,
  };
}
