import { LocationType } from '@/types/familyLocation.d';

/**
 * Function to get icon based on location type
 */
export const getLocationTypeIcon = (type: LocationType): string => {
  switch (type) {
    case LocationType.Grave:
      return 'mdi-grave-stone';
    case LocationType.Homeland:
      return 'mdi-home-map-marker';
    case LocationType.AncestralHall:
      return 'mdi-temple-hindu';
    case LocationType.Cemetery:
      return 'mdi-cemetery';
    case LocationType.EventLocation:
      return 'mdi-map-marker-account';
    case LocationType.Other:
      return 'mdi-map-marker';
    default:
      return 'mdi-map-marker';
  }
};