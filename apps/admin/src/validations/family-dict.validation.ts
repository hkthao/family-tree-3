import { useI18n } from 'vue-i18n';
import { required, helpers } from '@vuelidate/validators';
import { computed, type Ref } from 'vue';

export function useFamilyDictRules(_state: { [key: string]: Ref<any> }) {
  const { t } = useI18n();

  const rules = computed(() => {
    return {
      name: { required: helpers.withMessage(() => t('familyDict.form.rules.nameRequired'), required) },
      type: { required: helpers.withMessage(() => t('familyDict.form.rules.typeRequired'), required) },
      description: { required: helpers.withMessage(() => t('familyDict.form.rules.descriptionRequired'), required) },
      lineage: { required: helpers.withMessage(() => t('familyDict.form.rules.lineageRequired'), required) },
      namesByRegion: {
        north: { required: helpers.withMessage(() => t('familyDict.form.rules.northRequired'), required) },
        central: { required: helpers.withMessage(() => t('familyDict.form.rules.centralRequired'), required) },
        south: { required: helpers.withMessage(() => t('familyDict.form.rules.southRequired'), required) },
      },
    };
  });

  return rules;
}
