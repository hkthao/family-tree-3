import { ref, computed, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyAddDto } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyMutation } from '@/composables';
import type { FamilyForm as FamilyFormType } from '@/components/family';

interface UseFamilyAddDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  useAddFamilyMutation: typeof useAddFamilyMutation;
}

const defaultDeps: UseFamilyAddDeps = {
  useI18n,
  useGlobalSnackbar,
  useAddFamilyMutation,
};

export function useFamilyAdd(
  emit: (event: 'close' | 'saved') => void,
  familyFormRef: Ref<InstanceType<typeof FamilyFormType> | null>, // Moved here as direct parameter
  deps: UseFamilyAddDeps = defaultDeps,
) {
  const { useI18n, useGlobalSnackbar, useAddFamilyMutation } = deps;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: addFamily, isPending: isAddingFamily } = useAddFamilyMutation();

  const handleAddItem = async () => {
    if (!familyFormRef.value) return;
    const isValid = await familyFormRef.value.validate();
    if (!isValid) return;
    const itemData = familyFormRef.value.getFormData();
    addFamily(itemData as FamilyAddDto, {
      onSuccess: () => {
        showSnackbar(
          t('family.management.messages.addSuccess'),
          'success',
        );
        emit('close');
      },
      onError: (error) => {
        showSnackbar(
          error.message || t('common.saveError'),
          'error',
        );
      },
    });
  };

  const closeForm = () => {
    emit('close');
  };

  return {
    state: {
      familyFormRef,
      isAddingFamily: computed(() => isAddingFamily.value),
    },
    actions: {
      handleAddItem,
      closeForm,
    },
  };
}
