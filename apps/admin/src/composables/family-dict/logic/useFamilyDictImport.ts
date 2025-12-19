import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import type { FamilyDict } from '@/types';
import { useImportFamilyDictMutation } from '@/composables';

export function useFamilyDictImport(showDialogProp: boolean, emit: (event: 'update:show' | 'imported', ...args: any[]) => void) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const dialog = ref(showDialogProp);
  const selectedFile = ref<File[]>([]);
  const parsedData = ref<Omit<FamilyDict, 'id'>[] | null>(null);
  const parsedDataError = ref('');

  const { mutate: importFamilyDictsMutation, isPending: isImportingFamilyDicts } = useImportFamilyDictMutation();

  watch(() => showDialogProp, (newVal) => {
    dialog.value = newVal;
    if (!newVal) {
      resetState();
    }
  });

  watch(dialog, (newVal) => {
    emit('update:show', newVal);
  });

  const onFileSelected = (files: File[]) => {
    if (files && files.length > 0) {
      const file = files[0];
      if (file.type !== 'application/json') {
        parsedDataError.value = t('familyDict.import.errors.invalidFileType');
        parsedData.value = null;
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const content = e.target?.result as string;
          const data = JSON.parse(content);
          if (!Array.isArray(data) || data.some(item => !item.name || Number.isNaN(item.type) || Number.isNaN(item.lineage) || !item.namesByRegion || !item.namesByRegion.north)) {
            parsedDataError.value = t('familyDict.import.errors.invalidJsonStructure');
            parsedData.value = null;
          } else {
            parsedData.value = data;
            parsedDataError.value = '';
          }
        } catch (error) {
          parsedDataError.value = t('familyDict.import.errors.invalidJson');
          parsedData.value = null;
        }
      };
      reader.readAsText(file);
    } else {
      resetState();
    }
  };

  const importFamilyDicts = async () => {
    if (!parsedData.value) return;

    importFamilyDictsMutation({
      familyDicts: parsedData.value as FamilyDict[]
    }, {
      onSuccess: () => {
        showSnackbar(t('familyDict.import.messages.importSuccess'), 'success');
        emit('imported');
        closeDialog();
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyDict.import.messages.importError'), 'error');
      },
    });
  };

  const closeDialog = () => {
    dialog.value = false;
  };

  const resetState = () => {
    selectedFile.value = [];
    parsedData.value = null;
    parsedDataError.value = '';
  };

  return {
    dialog,
    selectedFile,
    parsedData,
    parsedDataError,
    isImportingFamilyDicts,
    onFileSelected,
    importFamilyDicts,
    closeDialog,
  };
}
