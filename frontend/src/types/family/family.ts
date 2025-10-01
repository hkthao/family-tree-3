import { FamilyVisibility } from './family-visibility';

export interface Family {
  id: string;
  name: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: FamilyVisibility;
}

export interface FamilyFilter {
  visibility?: 'all' | FamilyVisibility;
  searchQuery?: string;
  familyId?: string;
}

export interface ConfirmDeleteDialogProps {
  modelValue: boolean;
  title: string;
  message: string;
}
