import { FamilyVisibility } from "./family-visibility.d.ts";

export interface Family {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: FamilyVisibility;
  totalMembers?: number;
  totalGenerations?: number;
  validationErrors?: string[];
}

export interface FamilyFilter {
  visibility?: 'all' | FamilyVisibility;
  searchQuery?: string;
  familyId?: string;
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}

export interface FamilyUser {
    familyId: string;
    userProfileId: string;
    role: string;
}
