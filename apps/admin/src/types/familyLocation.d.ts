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

export interface Location extends BaseAuditableEntity {
  name: string;
  description?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  locationType: LocationType;
  accuracy: LocationAccuracy;
  source: LocationSource;
}

export interface FamilyLocation extends BaseAuditableEntity {
  familyId: string;
  locationId: string; // NEW: Added locationId
  location: Location; // NEW: Nested Location object
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type AddFamilyLocationDto = {
  familyId: string;
  locationName: string;
  locationDescription?: string;
  locationLatitude?: number;
  locationLongitude?: number;
  locationAddress?: string;
  locationType: LocationType;
  locationAccuracy: LocationAccuracy;
  locationSource: LocationSource;
};

export type UpdateFamilyLocationDto = {
  id: string; // FamilyLocation Id
  familyId: string; // FamilyLocation FamilyId
  locationId: string; // Location Id
  locationName: string;
  locationDescription?: string;
  locationLatitude?: number;
  locationLongitude?: number;
  locationAddress?: string;
  locationType: LocationType;
  locationAccuracy: LocationAccuracy;
  locationSource: LocationSource;
};

export type ImportFamilyLocationItemDto = {
  locationName: string;
  locationDescription?: string;
  locationLatitude?: number;
  locationLongitude?: number;
  locationAddress?: string;
  locationType: LocationType;
  locationAccuracy: LocationAccuracy;
  locationSource: LocationSource;
};

export interface FamilyLocationList extends PaginatedList<FamilyLocation> {}

export interface FamilyLocationFilter extends PaginationFilter {
  familyId?: string;
  locationType?: LocationType;
  locationSource?: LocationSource;
  searchQuery?: string;
}
