import { ref, onMounted, watch } from 'vue';
import type { FamilyMedia } from '@/types';
import { useFamilyMediaRules } from '@/validations/familyMedia.validation';

export function useFamilyMediaForm(initialMedia?: FamilyMedia) {
  const formRef = ref<HTMLFormElement | null>(null);
  const file = ref<File | undefined>(undefined);
  const description = ref<string | undefined>(undefined);

  const { rules } = useFamilyMediaRules();

  const getFormData = () => {
    return {
      file: file.value,
      description: description.value,
    };
  };

  const validate = async () => {
    if (formRef.value) {
      const { valid } = await formRef.value.validate();
      return valid;
    }
    return false;
  };

  const resetValidation = () => {
    if (formRef.value) {
      formRef.value.resetValidation();
    }
  };

  const resetForm = () => {
    file.value = undefined;
    description.value = undefined;
    resetValidation();
  };

  onMounted(() => {
    if (initialMedia) {
      description.value = initialMedia.description;
    }
  });

  watch(() => initialMedia, (newVal) => {
    if (newVal) {
      description.value = newVal.description;
    }
  }, { deep: true });

  return {
    formRef,
    file,
    description,
    formRules: rules, // Renamed to avoid conflict with `rules` within the component
    getFormData,
    validate,
    resetValidation,
    resetForm,
  };
}