// apps/admin/src/types/public-family.d.ts
import type { BaseAuditableDto } from './common-dto';

export enum FamilyRole {
  Manager = 0,
  Viewer = 1,
  Admin = 2,
}

export interface FamilyUserDto {
  familyId: string;
  userId: string;
  userName?: string;
  role: FamilyRole;
}

export interface FamilyDetailDto extends BaseAuditableDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  address?: string;
  avatarUrl?: string;
  visibility: string;
  totalMembers: number;
  totalGenerations: number;
  familyUsers: FamilyUserDto[];
}

export interface FamilyListDto extends BaseAuditableDto {
  id: string;
  name: string;
  description?: string;
  address?: string;
  avatarUrl?: string;
  totalMembers: number;
  totalGenerations: number;
  visibility: string;
}

export interface SearchPublicFamiliesQuery {
  page?: number;
  itemsPerPage?: number;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: string; // "asc" or "desc"
}
