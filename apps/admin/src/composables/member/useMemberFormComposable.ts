import { reactive, toRefs, ref, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import { Gender } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useMemberRules } from '@/validations/member.validation';
import { useAuth } from '@/composables/auth/useAuth';
import { getAvatarUrl } from '@/utils/avatar.utils';


interface UseMemberFormOptions {
  readOnly?: boolean;
  initialMemberData?: Member;
  familyId: string | null;
}

export function useMemberFormComposable(options: UseMemberFormOptions) {
  const { t } = useI18n();
  const { isAdmin, isFamilyManager } = useAuth();

  const formRef = ref<HTMLFormElement | null>(null);

  const isFormReadOnly = computed(() => {
    return options.readOnly || !(isAdmin.value || isFamilyManager.value);
  });

  const formData = reactive<Omit<Member, 'id'> | Member>(
    options.initialMemberData
      ? {
        ...options.initialMemberData,
        fatherId: options.initialMemberData.fatherId,
        motherId: options.initialMemberData.motherId,
        husbandId: options.initialMemberData.husbandId,
        wifeId: options.initialMemberData.wifeId,
        isRoot: options.initialMemberData.isRoot,
        isDeceased: options.initialMemberData.isDeceased,
        phone: options.initialMemberData.phone,
        email: options.initialMemberData.email,
        address: options.initialMemberData.address,
      }
      : {
        lastName: '',
        firstName: '',
        dateOfBirth: undefined,
        gender: Gender.Male,
        familyId: options.familyId || '',
        fatherId: undefined,
        motherId: undefined,
        husbandId: undefined,
        wifeId: undefined,
        isRoot: false,
        isDeceased: false,
        order: undefined,
        phone: undefined,
        email: undefined,
        address: undefined,
        avatarBase64: null,
      },
  );

  const state = reactive({
    lastName: toRef(formData, 'lastName'),
    firstName: toRef(formData, 'firstName'),
    dateOfBirth: toRef(formData, 'dateOfBirth'),
    dateOfDeath: toRef(formData, 'dateOfDeath'),
    familyId: toRef(formData, 'familyId'),
    fatherId: toRef(formData, 'fatherId'),
    motherId: toRef(formData, 'motherId'),
    husbandId: toRef(formData, 'husbandId'),
    wifeId: toRef(formData, 'wifeId'),
    isRoot: toRef(formData, 'isRoot'),
    isDeceased: toRef(formData, 'isDeceased'),
    order: toRef(formData, 'order'),
    phone: toRef(formData, 'phone'),
    email: toRef(formData, 'email'),
    address: toRef(formData, 'address'),
    avatarBase64: toRef(formData, 'avatarBase64'),
  });

  const rules = useMemberRules(toRefs(state));

  const v$ = useVuelidate(rules, state);

  const validate = async () => {
    const result = await v$.value.$validate();
    return result;
  };

  const getFormData = () => {
    return formData;
  };

  // Computed property to pass the initial avatar URL to AvatarInput
  const initialAvatarDisplay = computed(() => {
    return formData.avatarBase64 || formData.avatarUrl;
  });


  return {
    formRef,
    formData,
    v$,
    isFormReadOnly,
    initialAvatarDisplay,
    validate,
    getFormData,
    getAvatarUrl, // Make sure getAvatarUrl is accessible
  };
}
