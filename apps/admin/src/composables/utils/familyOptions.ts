import { type ComposerTranslation } from 'vue-i18n';
import { FamilyVisibility } from '@/types';

export function getFamilyVisibilityOptions(t: ComposerTranslation) {
  return [
    {
      title: t('family.form.visibility.private'),
      value: FamilyVisibility.Private,
    },
    { title: t('family.form.visibility.public'), value: FamilyVisibility.Public },
  ];
}