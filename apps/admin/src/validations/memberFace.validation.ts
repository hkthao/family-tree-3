import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';

export function useMemberFaceFormRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const memberFaceRules = {
    memberId: [rulesVuetify.required(t('memberFace.validation.memberIdRequired'))],
  };

  return memberFaceRules;
}
