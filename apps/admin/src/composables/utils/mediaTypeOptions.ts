import { type ComposerTranslation } from 'vue-i18n';
import { MediaType } from '@/types/enums';

/**
 * Generates media type options for a v-select component.
 * @param t The i18n translation function.
 * @returns An array of media type options.
 */
export function getMediaTypeOptions(t: ComposerTranslation) {
  return Object.values(MediaType)
    .filter(value => typeof value === 'number')
    .map(value => ({
      title: t(`common.mediaType.${MediaType[value as MediaType]}`),
      value: value as MediaType,
    }));
}