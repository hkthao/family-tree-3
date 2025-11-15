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
  address?: string;
  visibility?: FamilyVisibility;
  familyUsers?: FamilyUser[];
  totalMembers?: number;
  totalGenerations?: number;
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
  members: MemberExportDto[];
  relationships: RelationshipExportDto[];
  events: EventExportDto[];
}