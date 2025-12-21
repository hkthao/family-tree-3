import { ref, unref } from 'vue';
import type { Ref } from 'vue';
import { useGlobalSnackbar } from '@/composables';
import { useI18n } from 'vue-i18n';
import type { CreateMemberFaceCommand, UpdateMemberFaceCommand } from '@/types'; // Assuming these types exist

interface MemberFaceFormData {
  memberId: string;
  familyId: string;
  imageUrl: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

interface UseMemberFaceFormLogicOptions {
  mutation: (data: any) => Promise<any>;
  successMessageKey: string;
  errorMessageKey: string;
  formRef: Ref<InstanceType<any> | null>;
  onSuccess?: () => void;
  onError?: (error: Error) => void;
  transformData?: (data: MemberFaceFormData) => CreateMemberFaceCommand | UpdateMemberFaceCommand;
  isUpdate?: boolean;
}

export function useMemberFaceFormLogic(options: UseMemberFaceFormLogicOptions) {
  const { mutation, successMessageKey, errorMessageKey, formRef, onSuccess, onError, transformData } = options;
  const { showSnackbar } = useGlobalSnackbar();
  const { t } = useI18n();

  const isSubmitting = ref(false);

  const handleSubmit = async () => {
    if (!unref(formRef)) return;

    const isValid = await unref(formRef).validate();

    if (!isValid) {
      showSnackbar(t('common.form.validationError'), 'error');
      return;
    }

    const formData: MemberFaceFormData = unref(formRef).getFormData();

    const dataToSubmit = transformData ? transformData(formData) : formData;

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
    state: {
      isSubmitting,
    },
    actions: {
      handleSubmit,
    },
  };
}