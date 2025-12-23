import { computed, toRef, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyUpdateDto, FamilyAddDto } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useFamilyQuery, useUpdateFamilyMutation } from '@/composables';

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => FamilyAddDto | FamilyUpdateDto;
}

interface UseFamilyEditDeps {
  useI18n: typeof useI18n;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  useFamilyQuery: (familyIdRef: Ref<string | undefined>) => ReturnType<typeof useFamilyQuery>;
  useUpdateFamilyMutation: typeof useUpdateFamilyMutation;
}

const defaultDeps: UseFamilyEditDeps = {
  useI18n,
  useGlobalSnackbar,
  useFamilyQuery,
  useUpdateFamilyMutation,
};

export function useFamilyEdit(
  props: { familyId?: string },
  emit: (event: 'close' | 'saved') => void,
  familyFormRef: Ref<FamilyFormExposed | null>,
  deps: UseFamilyEditDeps = defaultDeps,
) {
  const { useI18n, useGlobalSnackbar, useFamilyQuery, useUpdateFamilyMutation } = deps;
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const familyIdRef = toRef(props, 'familyId') as Ref<string | undefined>;
  const { family: familyData, isLoading: isLoadingFamily, error } = useFamilyQuery(familyIdRef);
  const { mutate: updateFamily, isPending: isUpdatingFamily } = useUpdateFamilyMutation();

  const isLoading = computed(() => isLoadingFamily.value);

  const handleUpdateItem = async () => {
    if (!familyFormRef.value) return;
    const isValid = await familyFormRef.value.validate();
    if (!isValid) return;

    const itemData = familyFormRef.value.getFormData() as FamilyUpdateDto;

    updateFamily(itemData, {
      onSuccess: () => {
        showSnackbar(
          t('family.management.messages.updateSuccess'),
          'success',
        );
        emit('saved');
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
      family: familyData,
      isLoading,
      error,
      isUpdatingFamily: computed(() => isUpdatingFamily.value),
    },
    actions: {
      handleUpdateItem,
      closeForm,
    },
  };
}
