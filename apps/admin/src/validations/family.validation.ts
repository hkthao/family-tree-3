import { useI18n } from 'vue-i18n';
import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';

export function useFamilyRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required(t('common.validations.required'))],
    };
  });

  return rules;
}
