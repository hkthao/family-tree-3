export interface Family {
  id: string;
  name: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: 'public' | 'private' | 'shared';
}

export interface FamilyFilter {
  fullName?: string;
  visibility?: 'all' | 'public' | 'private';
}

export interface ConfirmDeleteDialogProps {
  modelValue: boolean;
  title: string;
  message: string;
}
