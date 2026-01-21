import { computed } from 'vue';
import { useI18n } from 'vue-i18n'; // Import useI18n
import { useRules } from 'vuetify/labs/rules';

export function useFamilyRules() {
  const { t } = useI18n(); // Use useI18n

  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      name: [rulesVuetify.required(t('common.validations.required'))],
      code: [rulesVuetify.required(t('common.validations.required'))], // Add code rule
    };
  });

  return rules;
}
