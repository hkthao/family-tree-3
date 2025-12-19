import { reactive, ref, computed } from 'vue';
import type { Member } from '@/types';
import { Gender } from '@/types';
import { useMemberRules } from '@/validations/member.validation';
import { useAuth } from '@/composables/auth/useAuth';
import { getAvatarUrl } from '@/utils/avatar.utils';


interface UseMemberFormOptions {
  readOnly?: boolean;
  initialMemberData?: Member;
  familyId: string | null;
}

export function useMemberFormComposable(options: UseMemberFormOptions) {
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

  const rules = useMemberRules(formData);

  const validate = async () => {
    if (!formRef.value) {
      return false;
    }
    const { valid } = await formRef.value.validate();
    return valid;
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
    isFormReadOnly,
    initialAvatarDisplay,
    validate,
    getFormData,
    getAvatarUrl,
    validationRules: rules, // Expose the rules
  };
}
