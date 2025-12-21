// src/composables/family-dict/useFamilyDictFormLogic.ts
import type { FamilyDict } from '@/types';
import { useAddFamilyDictMutation } from '@/composables';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';

// Define the interface for the functions that the composable needs from the form
interface FamilyDictFormActions {
  validate: () => Promise<boolean>;
  getFormData: () => Omit<FamilyDict, 'id'>;
}

interface UseFamilyDictFormLogicDeps {
  addFamilyDictMutation?: ReturnType<typeof useAddFamilyDictMutation>;
  globalSnackbar?: ReturnType<typeof useGlobalSnackbar>;
  t?: (key: string, ...args: any[]) => string; // Expect just the 't' function
  emit?: (event: 'saved' | 'close', ...args: any[]) => void;
  formActions?: FamilyDictFormActions; // Inject functions instead of ref
}

export function useFamilyDictFormLogic(deps: UseFamilyDictFormLogicDeps) {
  const {
    addFamilyDictMutation = useAddFamilyDictMutation(),
    globalSnackbar = useGlobalSnackbar(),
    t = useI18n().t, // Default to useI18n().t if not provided
    emit: injectedEmit,
    formActions, // Destructure formActions
  } = deps;

  const { mutate: addFamilyDict, isPending: isAddingFamilyDict } = addFamilyDictMutation;
  const { showSnackbar } = globalSnackbar;


  const handleAddFamilyDict = async () => {
    if (!formActions) {
      console.error('FamilyDictFormActions are not provided to useFamilyDictFormLogic.');
      return;
    }

    const isValid = await formActions.validate();

    if (!isValid) {
      return;
    }

    const familyDictData = formActions.getFormData();

    addFamilyDict(familyDictData, {
      onSuccess: () => {
        showSnackbar(t('familyDict.messages.addSuccess'), 'success');
        injectedEmit?.('saved');
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyDict.messages.saveError'), 'error');
      },
    });
  };

  const closeForm = () => {
    injectedEmit?.('close');
  };

  return {
    state: {
      isAddingFamilyDict,
    },
    actions: {
      handleAddFamilyDict,
      closeForm,
    },
  };
}
