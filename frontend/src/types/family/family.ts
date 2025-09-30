import { FamilyVisibility } from './family-visibility';

export interface Family {
  id: string;
  name: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: FamilyVisibility;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface FamilyFilter {
  name?: string;
  description?: string;
  address?: string;
  visibility?: 'all' | FamilyVisibility;
  createdAtStart?: Date;
  createdAtEnd?: Date;
  updatedAtStart?: Date;
  updatedAtEnd?: Date;
  searchQuery?: string;
  familyId?: string;
  startDate?: Date;
  endDate?: Date;
  location?: string;
  type?: string;
}

export interface ConfirmDeleteDialogProps {
  modelValue: boolean;
  title: string;
  message: string;
}
