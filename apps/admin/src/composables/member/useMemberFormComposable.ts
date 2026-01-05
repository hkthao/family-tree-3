import { reactive, ref, computed } from 'vue';
import type { MemberDto, MemberAddDto, MemberUpdateDto } from '@/types';
import { Gender } from '@/types';
import { useMemberRules } from '@/validations/member.validation';
import { useAuth } from '@/composables/auth/useAuth';
import { getAvatarUrl } from '@/utils/avatar.utils';


interface UseMemberFormOptions {
  readOnly?: boolean;
  initialMemberData?: MemberDto | null;
  familyId: string | null;
}

export function useMemberFormComposable(options: UseMemberFormOptions) {
  const { state } = useAuth();

  const formRef = ref<HTMLFormElement | null>(null);

  const isFormReadOnly = computed(() => {
    return options.readOnly || !(state.isAdmin.value || state.isFamilyManager.value);
  });

  const formData = reactive<MemberAddDto | MemberUpdateDto>(
    (options.initialMemberData
      ? {
          id: options.initialMemberData.id,
          lastName: options.initialMemberData.lastName,
          firstName: options.initialMemberData.firstName,
          code: options.initialMemberData.code,
          familyId: options.initialMemberData.familyId,
          gender: options.initialMemberData.gender,
          dateOfBirth: options.initialMemberData.dateOfBirth ? new Date(options.initialMemberData.dateOfBirth) : undefined,
          dateOfDeath: options.initialMemberData.dateOfDeath ? new Date(options.initialMemberData.dateOfDeath) : undefined,
          avatarUrl: options.initialMemberData.avatarUrl,
          avatarBase64: options.initialMemberData.avatarBase64,
          nickname: options.initialMemberData.nickname,
          placeOfBirth: options.initialMemberData.placeOfBirth,
          placeOfDeath: options.initialMemberData.placeOfDeath,
          birthLocationId: options.initialMemberData.birthLocationId, // Added
          deathLocationId: options.initialMemberData.deathLocationId, // Added
          residenceLocationId: options.initialMemberData.residenceLocationId, // Added
          phone: options.initialMemberData.phone,
          email: options.initialMemberData.email,
          address: options.initialMemberData.address,
          occupation: options.initialMemberData.occupation,
          biography: options.initialMemberData.biography,
          isRoot: options.initialMemberData.isRoot,
          isDeceased: options.initialMemberData.isDeceased,
          fatherId: options.initialMemberData.fatherId,
          motherId: options.initialMemberData.motherId,
          husbandId: options.initialMemberData.husbandId,
          wifeId: options.initialMemberData.wifeId,
          order: options.initialMemberData.order,
        } as MemberUpdateDto
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
          occupation: undefined,
          nickname: undefined,
          placeOfBirth: undefined,
          placeOfDeath: undefined,
          birthLocationId: undefined, // Added
          deathLocationId: undefined, // Added
          residenceLocationId: undefined, // Added
          biography: undefined,
        } as MemberAddDto)
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