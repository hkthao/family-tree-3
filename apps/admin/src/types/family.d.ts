import type { FilterOptions } from './pagination'; // Added explicit import
export enum FamilyVisibility {
  Private = 'Private',
  Public = 'Public',
  Shared = 'Shared',
}

export interface Family {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  avatarBase64?: string | null;
  address?: string;
  visibility?: FamilyVisibility;
  familyUsers?: FamilyUser[];
  totalMembers?: number;
  totalGenerations?: number;
  managerIds: string[]; // Changed to string[]
  viewerIds: string[]; // Changed to string[]
  familyLimitConfiguration?: FamilyLimitConfiguration;

}

export interface FamilyAddDto {
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  avatarBase64?: string | null;
  address?: string;
  visibility?: FamilyVisibility;
  managerIds: string[];
  viewerIds: string[];
  familyLimitConfiguration?: FamilyLimitConfiguration;
}

export interface FamilyUpdateDto {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  avatarBase64?: string | null;
  address?: string;
  visibility?: FamilyVisibility;
  managerIds: string[];
  viewerIds: string[];
  deletedManagerIds: string[]; // New field
  deletedViewerIds: string[]; // New field
  familyLimitConfiguration?: FamilyLimitConfiguration;
}

export interface IFamilyAccess {
  familyId: string;
  role: number; // Corresponds to FamilyRole enum in backend
}

export interface FamilyFilter extends FilterOptions {
  visibility?: 'all' | FamilyVisibility;
  searchQuery?: string;
  familyId?: string;
  page?: number;
  itemsPerPage?: number;
  sortBy?: { key: string; order: 'asc' | 'desc' }[];
}

export interface FamilyUser {
  userId: string;
  role: number;
}

export interface FamilyLimitConfiguration {
  id: string;
  familyId: string;
  maxMembers: number;
  maxStorageMb: number;
  aiChatMonthlyLimit: number;
  aiChatMonthlyUsage: number;
}

export interface MemberExportDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  nickname?: string;
  gender: number;
  dateOfBirth?: Date;
  dateOfDeath?: Date;
  placeOfBirth?: string;
  placeOfDeath?: string;
  occupation?: string;
  biography?: string;
  avatarUrl?: string;
  isRoot: boolean;
  order: number;
}

export interface RelationshipExportDto {
  id: string;
  sourceMemberId: string;
  targetMemberId: string;
  relationshipType: number;
  startDate?: Date;
  endDate?: Date;
  description?: string;
}

export interface EventExportDto {
  id: string;
  name: string;
  type: number;
  startDate: Date;
  endDate?: Date;
  location?: string;
  description?: string;
  color?: string;
  relatedMemberIds: string[];
}

export interface FamilyExportDto {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility: number;
  familyUsers?: any[]; // Added for consistency with test expectations
  settings?: any; // Added for consistency with test expectations
  privacyConfiguration?: any; // Added for consistency with test expectations
  familyLimitConfiguration?: FamilyLimitConfiguration;
  members: MemberExportDto[];
  relationships: RelationshipExportDto[];
  events: EventExportDto[];
}