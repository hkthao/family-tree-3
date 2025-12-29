import type { BaseAuditableEntity, PaginatedList, PaginationFilter } from '@/types';

export enum LocationType {
  Grave = 0,
  Homeland = 1,
  AncestralHall = 2,
  Cemetery = 3,
  EventLocation = 4,
  Other = 5,
}

export enum LocationAccuracy {
  Exact = 0,
  Approximate = 1,
  Estimated = 2,
}

export enum LocationSource {
  UserSelected = 0,
  Geocoded = 1,
}

export interface FamilyLocation extends BaseAuditableEntity {
  familyId: string;
  name: string;
  description?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  locationType: LocationType;
  accuracy: LocationAccuracy;
  source: LocationSource;
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type AddFamilyLocationDto = Omit<FamilyLocation, 'id'>;
export type UpdateFamilyLocationDto = FamilyLocation;

export interface FamilyLocationList extends PaginatedList<FamilyLocation> {}

export interface FamilyLocationFilter extends PaginationFilter {
  familyId?: string;
  locationType?: LocationType;
  locationSource?: LocationSource;
}
