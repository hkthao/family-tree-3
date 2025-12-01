import { required, helpers } from '@vuelidate/validators';
import i18n from '@/plugins/i18n';

const { t } = i18n.global;

export function useMemberFaceFormRules() {
  const memberFaceRules = {
    memberId: {
      required: helpers.withMessage(t('memberFace.validation.memberIdRequired'), required),
    },
  };

  return memberFaceRules;
}
