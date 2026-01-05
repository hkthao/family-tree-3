// Enums

export enum LocationLinkType {
  Birth = 'Birth',
  Death = 'Death',
  Residence = 'Residence',
  General = 'General',
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
