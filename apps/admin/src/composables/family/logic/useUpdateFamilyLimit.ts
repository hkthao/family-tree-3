import { reactive, computed, type Ref, toRef } from 'vue';
import { useFamilyLimitConfiguration } from '@/composables/family/logic/useFamilyLimitConfiguration';
import type FamilyLimitConfigForm from '@/components/family/FamilyLimitConfigForm.vue';

interface UpdateFamilyLimitProps {
  familyId: string;
}

interface UpdateFamilyLimitEmits {
  (e: 'close'): void;
  (e: 'saved'): void;
}

export function useUpdateFamilyLimit(
  props: UpdateFamilyLimitProps,
  emit: UpdateFamilyLimitEmits,
  familyLimitConfigFormRef: Ref<InstanceType<typeof FamilyLimitConfigForm> | null>
) {
  const familyIdRef = toRef(props, 'familyId') as Ref<string>;

  const { isLoading, error, familyLimitData, isUpdating, updateFamilyLimits } =
    useFamilyLimitConfiguration(familyIdRef); // Pass the Ref directly

  const handleActionSave = async () => {
    if (!familyLimitConfigFormRef.value) return;
    const isValid = await familyLimitConfigFormRef.value.validate();
    if (!isValid) return;

    const formData = familyLimitConfigFormRef.value.formData;
    updateFamilyLimits({
      familyId: props.familyId,
      maxMembers: formData.maxMembers,
      maxStorageMb: formData.maxStorageMb,
      aiChatMonthlyLimit: formData.aiChatMonthlyLimit,
    }, {
      onSuccess: () => {
        emit('saved');
        // Removed emit('close') to keep the drawer open
      },
      onError: () => {
        emit('close'); // Close on error as well, or handle error display more explicitly
      },
    });
  };

  const closeForm = () => emit('close');

  const state = reactive({
    isLoading: computed(() => isLoading.value),
    error: computed(() => error.value),
    familyLimitData: computed(() => familyLimitData.value),
    isUpdating: computed(() => isUpdating.value),
  });

  const actions = {
    handleActionSave,
    closeForm,
  };

  return {
    state,
    actions,
  };
}
