import { ref, computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family, FamilyUser } from '@/types';
import { FamilyVisibility } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useFamilyRules } from '@/validations/family.validation';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';

export function useFamilyForm(props: { data?: Family; readOnly?: boolean }, emit: (event: 'submit', data: Family | Omit<Family, 'id'>) => void) {
  const { t } = useI18n();

  const formData = reactive<Family | Omit<Family, 'id'>>(
    props.data ? JSON.parse(JSON.stringify(props.data)) : {
      name: '',
      description: '',
      address: '',
      avatarUrl: '',
      avatarBase64: null,
      visibility: FamilyVisibility.Public,
      familyUsers: [],
    },
  );

  const initialAvatarDisplay = computed(() => {
    return formData.avatarBase64 || formData.avatarUrl;
  });

  watch(
    () => props.data,
    (newVal) => {
      if (newVal) {
        Object.assign(formData, newVal);
        familyUsers.value = newVal.familyUsers || [];
        formData.avatarBase64 = null;
      }
    },
    { deep: true }
  );

  const rules = useFamilyRules();
  const v$ = useVuelidate(rules, formData);

  const familyUsers = ref<FamilyUser[]>(props.data?.familyUsers || []);
  const Manager = 0;
  const Viewer = 1;

  const managers = computed({
    get: () => familyUsers.value.filter(fu => fu.role === Manager).map(fu => fu.userId),
    set: (newUserIds: string[]) => {
      const newManagers = newUserIds.map(userId => ({ userId: userId, role: Manager }));
      const otherUsers = familyUsers.value.filter(fu => fu.role !== Manager);
      familyUsers.value = [...otherUsers, ...newManagers];
    }
  });

  const viewers = computed({
    get: () => familyUsers.value.filter(fu => fu.role === Viewer).map(fu => fu.userId),
    set: (newUserIds: string[]) => {
      const newViewers = newUserIds.map(userId => ({ userId: userId, role: Viewer }));
      const otherUsers = familyUsers.value.filter(fu => fu.role !== Viewer);
      familyUsers.value = [...otherUsers, ...newViewers];
    }
  });

  const visibilityItems = computed(() => [
    {
      title: t('family.form.visibility.private'),
      value: FamilyVisibility.Private,
    },
    { title: t('family.form.visibility.public'), value: FamilyVisibility.Public },
  ]);

  const submitForm = async () => {
    const result = await v$.value.$validate();
    if (result) {
      emit('submit', formData);
    }
  };

  const validate = async () => {
    const result = await v$.value.$validate();
    return result;
  };

  const getFormData = () => {
    const dataToSubmit = { ...formData };
    dataToSubmit.familyUsers = familyUsers.value;
    return dataToSubmit;
  };

  return {
    t,
    formData,
    initialAvatarDisplay,
    v$,
    familyUsers, // Expose familyUsers if needed for parent to manipulate directly
    managers,
    viewers,
    visibilityItems,
    submitForm,
    validate,
    getFormData,
    getFamilyAvatarUrl, // Expose utility function
  };
}
