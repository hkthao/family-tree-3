import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRules } from 'vuetify/labs/rules';

export function useChatDrawerRules() {
  const { t } = useI18n();
  const rulesVuetify = useRules();

  const rules = computed(() => {
    return {
      selectedFamilyForChat: [(v: string | null | undefined) => !!v || t('chat.drawer.validation.selectFamilyRequired')],
    };
  });

  return { rules };
}
