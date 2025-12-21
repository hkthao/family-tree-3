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

  const parseFamilyDictFile = (file: File, t: (key: string) => string): Promise<{ data: Omit<FamilyDict, 'id'>[] | null, error: string }> => {
    return new Promise((resolve) => {
      if (file.type !== 'application/json') {
        resolve({ data: null, error: t('familyDict.import.errors.invalidFileType') });
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const content = e.target?.result as string;
          const data = JSON.parse(content);
          if (!Array.isArray(data) || data.some(item => !item.name || Number.isNaN(item.type) || Number.isNaN(item.lineage) || !item.namesByRegion || !item.namesByRegion.north)) {
            resolve({ data: null, error: t('familyDict.import.errors.invalidJsonStructure') });
          } else {
            resolve({ data: data, error: '' });
          }
        } catch (error) {
          resolve({ data: null, error: t('familyDict.import.errors.invalidJson') });
        }
      };
      reader.readAsText(file);
    });
  };

  const onFileSelected = async (files: File[]) => {
    if (files && files.length > 0) {
      const file = files[0];
      const { data, error } = await parseFamilyDictFile(file, t);
      parsedData.value = data;
      parsedDataError.value = error;
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
    state: {
      dialog,
      selectedFile,
      parsedData,
      parsedDataError,
      isImportingFamilyDicts,
    },
    actions: {
      onFileSelected,
      importFamilyDicts,
      closeDialog,
    },
  };
}
