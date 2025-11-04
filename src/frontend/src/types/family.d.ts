import { FamilyVisibility } from "./family-visibility.d.ts";

export interface Family {
  id: string;
  name: string;
  code?: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: FamilyVisibility;
  familyUsers?: FamilyUser[];
}

export interface FamilyFilter {
  visibility?: 'all' | FamilyVisibility;
  searchQuery?: string;
  familyId?: string;
  sortBy?: string; // Column name to sort by
  sortOrder?: 'asc' | 'desc'; // Sort order
}

export interface FamilyUser {
    userId: string;
    role: number;
}
