import type { FilterOptions } from './pagination'; // Added explicit import
export enum FamilyVisibility {
  Private = 'Private',
  Public = 'Public',
  Shared = 'Shared',
}

export interface FamilyDto {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  avatarBase64?: string | null;
  address?: string;
  genealogyRecord?: string;
  progenitorName?: string;
  familyCovenant?: string;
  contactInfo?: string;
  locationId?: string | null; // Added locationId
  visibility?: FamilyVisibility;
  familyUsers?: FamilyUser[];
  totalMembers?: number;
  totalGenerations?: number;
  managerIds: string[]; // Changed to string[]
  viewerIds: string[]; // Changed to string[]
  familyLimitConfiguration?: FamilyLimitConfiguration;
  validationErrors?: string[];
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export interface FamilyAddDto {
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  avatarBase64?: string | null;
  address?: string;
  genealogyRecord?: string;
  progenitorName?: string;
  familyCovenant?: string;
  contactInfo?: string;
  locationId?: string | null; // Added locationId
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
  genealogyRecord?: string;
  progenitorName?: string;
  familyCovenant?: string;
  contactInfo?: string;
  locationId?: string | null; // Added locationId
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
  isFollowing?: boolean; // NEW
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

export interface UpdateFamilyLimitConfigurationDto {
  familyId: string;
  maxMembers: number;
  maxStorageMb: number;
  aiChatMonthlyLimit: number;
}

export interface FamilyImportDto {
  families: FamilyAddDto[];
}
