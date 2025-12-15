import { ref, unref } from 'vue';
import type { Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';

interface FamilyMediaFormData {
  file?: File;
  description?: string;
  mediaId?: string; // For update operations
}

interface UseFamilyMediaFormLogicOptions {
  familyId: Ref<string>;
  mutation: (data: any) => Promise<any>;
  successMessageKey: string;
  errorMessageKey: string;
  formRef: Ref<InstanceType<any> | null>;
  onSuccess?: () => void;
  onError?: (error: Error) => void;
  transformData?: (data: FamilyMediaFormData, familyId: string) => any;
  isUpdate?: boolean;
}

export function useFamilyMediaFormLogic(options: UseFamilyMediaFormLogicOptions) {
  const { familyId, mutation, successMessageKey, errorMessageKey, formRef, onSuccess, onError, transformData, isUpdate = false } = options;
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  const isSubmitting = ref(false);

  const handleSubmit = async () => {
    if (!unref(formRef) || !unref(familyId)) return;

    const isValid = await unref(formRef).validate();

    if (!isValid) {
      showSnackbar(t('common.form.validationError'), 'error');
      return;
    }

    const formData: FamilyMediaFormData = unref(formRef).getFormData();

    if (!isUpdate && !formData.file) { // File is required for add, but not necessarily for update if description is only thing changing
      showSnackbar(t('familyMedia.errors.noFileSelected'), 'error');
      return;
    }

    const dataToSubmit = transformData ? transformData(formData, unref(familyId)) : { familyId: unref(familyId), ...formData };

    isSubmitting.value = true;
    try {
      await mutation(dataToSubmit);
      showSnackbar(t(successMessageKey), 'success');
      onSuccess?.();
    } catch (error: any) {
      showSnackbar(error.message || t(errorMessageKey), 'error');
      onError?.(error);
    } finally {
      isSubmitting.value = false;
    }
  };

  return {
    isSubmitting,
    handleSubmit,
  };
}