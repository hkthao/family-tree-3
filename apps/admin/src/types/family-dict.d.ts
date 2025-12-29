export enum FamilyDictType {
  Blood = 0,
  Marriage = 1,
  Adoption = 2,
  InLaw = 3,
  Other = 4,
}

export enum FamilyDictLineage {
  Noi = 0,
  Ngoai = 1,
  NoiNgoai = 2,
  Other = 3,
}

export interface NamesByRegion {
  north: string;
  central: string | string[];
  south: string | string[];
}

export interface FamilyDict {
  id: string;
  name: string;
  type: FamilyDictType;
  description: string;
  lineage: FamilyDictLineage;
  specialRelation: boolean;
  namesByRegion: NamesByRegion;
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type AddFamilyDictDto = Omit<FamilyDict, 'id'>;
export type UpdateFamilyDictDto = FamilyDict;

export interface FamilyDictImport {
  familyDicts: FamilyDict[];
}

export interface FamilyDictFilter {
  searchQuery?: string;
  lineage?: FamilyDictLineage;
  region?: string;
  sortBy?: { key: string; order: 'asc' | 'desc' }[]; // Updated type
  sortOrder?: 'asc' | 'desc'; // Keep this for backward compatibility if needed, but sortBy should cover it
  page?: number;
  itemsPerPage?: number;
}
