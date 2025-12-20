import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyAddDto } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyMutation } from '@/composables';
import type { FamilyForm } from '@/components/family';

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

export function useFamilyAdd(emit: (event: 'close' | 'saved') => void, deps: UseFamilyAddDeps = defaultDeps) {
  const { useI18n, useGlobalSnackbar, useAddFamilyMutation } = deps;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();
  const { mutate: addFamily, isPending: isAddingFamily } = useAddFamilyMutation();

  const familyFormRef = ref<InstanceType<typeof FamilyForm> | null>(null);

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
