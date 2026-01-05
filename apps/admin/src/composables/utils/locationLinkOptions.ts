import { type ComposerTranslation } from 'vue-i18n';
import { LocationLinkType, RefType, type LocationLinkDto } from '@/types'; // Import necessary types

/**
 * Returns a descriptive string for a given LocationLinkDto.
 * @param t Translation function from vue-i18n.
 * @param locationLink The LocationLinkDto object.
 * @returns A descriptive string for the location link.
 */
export function getLocationLinkDescription(t: ComposerTranslation, locationLink: LocationLinkDto): string {
  let description = '';
  switch (locationLink.linkType) {
    case LocationLinkType.Birth:
      description = t('familyLocation.locationLinkType.birth');
      break;
    case LocationLinkType.Death:
      description = t('familyLocation.locationLinkType.death');
      break;
    case LocationLinkType.Residence:
      description = t('familyLocation.locationLinkType.residence');
      break;
    case LocationLinkType.General:
      description = t('familyLocation.locationLinkType.general');
      break;
    default:
      description = locationLink.description || '';
  }
  if (locationLink.refType === RefType.Member) {
    description += ` (${t('common.member')})`;
  } else if (locationLink.refType === RefType.Event) {
    description += ` (${t('common.event')})`;
  } else if (locationLink.refType === RefType.Family) {
    description += ` (${t('common.family')})`;
  }
  return description;
}
