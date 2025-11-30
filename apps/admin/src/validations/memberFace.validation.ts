import { required, helpers, minValue, maxValue } from '@vuelidate/validators';
import i18n from '@/plugins/i18n';

const { t } = i18n.global;

export function useMemberFaceFormRules() {
  const memberFaceRules = {
    memberId: {
      required: helpers.withMessage(t('memberFace.validation.memberIdRequired'), required),
    },
    faceId: {
      required: helpers.withMessage(t('memberFace.validation.faceIdRequired'), required),
    },
    boundingBox: {
      x: {
        required: helpers.withMessage(t('memberFace.validation.boundingBoxRequired'), required),
        minValue: helpers.withMessage(t('memberFace.validation.minValue', { min: 0 }), minValue(0)),
      },
      y: {
        required: helpers.withMessage(t('memberFace.validation.boundingBoxRequired'), required),
        minValue: helpers.withMessage(t('memberFace.validation.minValue', { min: 0 }), minValue(0)),
      },
      width: {
        required: helpers.withMessage(t('memberFace.validation.boundingBoxRequired'), required),
        minValue: helpers.withMessage(t('memberFace.validation.minValue', { min: 1 }), minValue(1)),
      },
      height: {
        required: helpers.withMessage(t('memberFace.validation.boundingBoxRequired'), required),
        minValue: helpers.withMessage(t('memberFace.validation.minValue', { min: 1 }), minValue(1)),
      },
    },
    confidence: {
      required: helpers.withMessage(t('memberFace.validation.confidenceRequired'), required),
      minValue: helpers.withMessage(t('memberFace.validation.minValue', { min: 0 }), minValue(0)),
      maxValue: helpers.withMessage(t('memberFace.validation.maxValue', { max: 1 }), maxValue(1)),
    },
    embedding: {
        required: helpers.withMessage(t('memberFace.validation.embeddingRequired'), required),
    }
  };

  return memberFaceRules;
}
