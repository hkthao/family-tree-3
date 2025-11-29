import { required, minLength } from '@vuelidate/validators';
import i18n from '@/plugins/i18n'; // Correct import path

const { t } = i18n.global;

export const memberStoryValidationRules = {
  memberId: {
    required: {
      $validator: required,
      $message: () => t('common.validations.required'),
    },
  },
  rawInput: {
    minLength: {
      $validator: minLength(10),
      $message: ({ $params }: { $params: { min: number } }) => t('memberStory.form.rules.rawInputMinLength', { length: $params.min }),
    },
  },
  title: {
    required: {
      $validator: required,
      $message: () => t('common.validations.required'),
    },
  },
  story: {
    required: {
      $validator: required,
      $message: () => t('common.validations.required'),
    },
  },
};
