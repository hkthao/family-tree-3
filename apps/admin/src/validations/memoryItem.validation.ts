import { computed } from 'vue';
import { useRules } from 'vuetify/labs/rules';
import { EmotionalTag } from '@/types';

export function useMemoryItemRules() {
  const rulesVuetify = useRules();

  const maxFiles = (value: File[] | undefined) => {
    if (!value) return true;
    return value.length <= 5 || 'Maximum 5 files allowed.';
  };

  const rules = computed(() => {
    return {
      title: [rulesVuetify.required()],
      emotionalTag: [rulesVuetify.required()],
      uploadedFiles: [maxFiles],
    };
  });

  return { rules };
}