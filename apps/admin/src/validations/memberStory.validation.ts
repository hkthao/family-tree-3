import { required, minLength, requiredIf } from '@vuelidate/validators';
import i18n from '@/plugins/i18n'; // Correct import path

const { t } = i18n.global;

export const memberStoryValidationRules = {
  memberId: {
    required: {
      $validator: required,
      $message: () => t('memberStory.form.rules.memberIdRequired'),
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
      $message: () => t('memberStory.form.rules.titleRequired'),
    },
  },
  story: {
    required: {
      $validator: required,
      $message: () => t('memberStory.form.rules.storyRequired'),
    },
  },
  photoUrl: {
    required: {
      $validator: requiredIf(function (this: any) {
        return !this.rawInput;
      }),
      $message: () => t('memberStory.form.rules.photoUrlRequiredIfRawInputEmpty'),
    },
  },
};
