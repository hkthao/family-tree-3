import { useRules } from 'vuetify/labs/rules';

export function useMemberFaceFormRules() {
  const rulesVuetify = useRules();

  const memberFaceRules = {
    memberId: [rulesVuetify.required()],
  };

  return memberFaceRules;
}
