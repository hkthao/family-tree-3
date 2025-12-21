import type { Ref } from 'vue'; // Import Ref
import type { Prompt } from '@/types';
import type { ComposerTranslation } from 'vue-i18n';

import type { PromptForm as PromptFormType } from '@/components/prompt';
import { useGlobalSnackbar } from '@/composables';
import type { UseUpdatePromptMutationReturn } from '@/composables/prompt/mutations/useUpdatePromptMutation';

interface UsePromptFormLogicOptions {
  promptFormRef: Ref<InstanceType<typeof PromptFormType> | null>;
  t: ComposerTranslation;
  showSnackbar: ReturnType<typeof useGlobalSnackbar>['showSnackbar'];
  updatePromptMutation: UseUpdatePromptMutationReturn; // Use the imported return type
  onSaved: () => void;
  onClosed: () => void;
}

export function usePromptFormLogic(options: UsePromptFormLogicOptions) {
  const { promptFormRef, t, showSnackbar, updatePromptMutation, onSaved, onClosed } = options;
  const { state: { isPending: isUpdatingPrompt }, actions: { updatePrompt } } = updatePromptMutation;

  const handleUpdatePrompt = async () => {
    if (!promptFormRef.value) return;
    const isValid = await promptFormRef.value.validate();

    if (!isValid) {
      return;
    }

    const promptData = promptFormRef.value.getFormData() as Prompt;
    if (!promptData.id) {
      showSnackbar(t('prompt.messages.saveError'), 'error');
      return;
    }

    updatePrompt(promptData, {
      onSuccess: () => {
        showSnackbar(t('prompt.messages.updateSuccess'), 'success');
        onSaved();
      },
      onError: (error: Error) => {
        showSnackbar(error.message || t('prompt.messages.saveError'), 'error');
      },
    });
  };

  const closeForm = () => {
    onClosed();
  };

  return {
    state: {
      isUpdatingPrompt,
    },
    actions: {
      handleUpdatePrompt,
      closeForm,
    },
  };
}