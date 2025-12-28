// apps/admin/src/composables/prompt/usePromptImportExport.ts

import { ref, computed } from 'vue';
import { useServices } from '@/plugins/services.plugin';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useI18n } from 'vue-i18n';
import type { Prompt, AddPromptDto } from '@/types/prompt';
import type { ApiError } from '@/types/api';

export function usePromptImportExport() {
  const { t } = useI18n();
  const { prompt: promptService } = useServices();
  const { showSnackbar } = useGlobalSnackbar();

  const isExporting = ref(false);
  const isImporting = ref(false);
  const exportError = ref<ApiError | null>(null);
  const importError = ref<ApiError | null>(null);

  const exportPrompts = async () => {
    isExporting.value = true;
    exportError.value = null;
    try {
      const result = await promptService.exportPrompts();
      if (result.ok) {
        const promptsToExport = result.value;
        const jsonString = JSON.stringify(promptsToExport, null, 2);
        const blob = new Blob([jsonString], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `prompts-export-${new Date().toISOString().slice(0, 10)}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        showSnackbar(t('prompt.messages.exportSuccess'), 'success');
      } else {
        exportError.value = result.error;
        showSnackbar(result.error.message || t('prompt.messages.exportError'), 'error');
      }
    } catch (err: any) {
      exportError.value = err;
      showSnackbar(err.message || t('prompt.messages.exportError'), 'error');
    } finally {
      isExporting.value = false;
    }
  };

  const importPrompts = async (prompts: AddPromptDto[]) => {
    isImporting.value = true;
    importError.value = null;
    try {
      const result = await promptService.importPrompts(prompts);
      if (result.ok) {
        showSnackbar(t('prompt.messages.importSuccess', { count: result.value.length }), 'success');
        return true;
      } else {
        importError.value = result.error;
        showSnackbar(result.error.message || t('prompt.messages.importError'), 'error');
        return false;
      }
    } catch (err: any) {
      importError.value = err;
      showSnackbar(err.message || t('prompt.messages.importError'), 'error');
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
    exportPrompts,
    importPrompts,
  };
}
