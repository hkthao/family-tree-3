import { ref, computed, reactive, watch, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family, FamilyUser } from '@/types';
import { FamilyVisibility } from '@/types';
import { useFamilyRules } from '@/validations/family.validation';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import type { VForm } from 'vuetify/components';

export function useFamilyForm(props: { data?: Family; readOnly?: boolean }, emit: (event: 'submit', data: Family | Omit<Family, 'id'>) => void, formRef: Ref<VForm | null>) {
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

  const validate = async () => {
    return (await formRef.value?.validate())?.valid || false;
  };

  const submitForm = async () => {
    const isValid = await validate();
    if (isValid) {
      emit('submit', formData);
    }
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
    familyUsers,
    managers,
    viewers,
    visibilityItems,
    submitForm,
    validate,
    getFormData,
    getFamilyAvatarUrl,
    rules, // Expose rules for the template
  };
}
