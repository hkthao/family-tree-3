import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useProfileSettingsRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules()
  const rules = computed(() => {
    return {
      firstName: [rulesVuetify.required(t('common.validations.required'))],
      lastName: [rulesVuetify.required(t('common.validations.required'))],
      email: [
        rulesVuetify.required(t('common.validations.required')),
        rulesVuetify.email(t('common.validations.email')),
      ],
    };
  });

  return rules;
}