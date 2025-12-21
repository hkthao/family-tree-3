import { type ComposerTranslation } from 'vue-i18n';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';

export function getLocationTypeOptions(t: ComposerTranslation) {
  return [
    { title: t('familyLocation.locationType.grave'), value: LocationType.Grave },
    { title: t('familyLocation.locationType.homeland'), value: LocationType.Homeland },
    { title: t('familyLocation.locationType.ancestralHall'), value: LocationType.AncestralHall },
    { title: t('familyLocation.locationType.cemetery'), value: LocationType.Cemetery },
    { title: t('familyLocation.locationType.eventLocation'), value: LocationType.EventLocation },
    { title: t('familyLocation.locationType.other'), value: LocationType.Other },
  ];
}

export function getLocationAccuracyOptions(t: ComposerTranslation) {
  return [
    { title: t('familyLocation.accuracy.exact'), value: LocationAccuracy.Exact },
    { title: t('familyLocation.accuracy.approximate'), value: LocationAccuracy.Approximate },
    { title: t('familyLocation.accuracy.estimated'), value: LocationAccuracy.Estimated },
  ];
}

export function getLocationSourceOptions(t: ComposerTranslation) {
  return [
    { title: t('familyLocation.source.userselected'), value: LocationSource.UserSelected },
    { title: t('familyLocation.source.geocoded'), value: LocationSource.Geocoded },
  ];
}