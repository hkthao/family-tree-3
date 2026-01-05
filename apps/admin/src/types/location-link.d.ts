// Enums

export enum LocationLinkType {
  Birth = 0,
  Death = 1,
  Residence = 2,
  General = 3,
}

// DTOs
export interface LocationDto {
  id: string;
  name: string;
  description?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  locationType: string;
  accuracy: string;
  source: string;
}

export interface LocationLinkDto {
  id: string;
  refId: string;
  refType: RefType;
  description: string;
  locationId: string;
  linkType: LocationLinkType;
  location?: LocationDto; // Optional, as it might not always be included
}
