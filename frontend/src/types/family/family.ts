export interface Family {
  id: string;
  name: string;
  description?: string;
  avatarUrl?: string;
  address?: string;
  visibility?: 'public' | 'private' | 'shared';
  createdAt?: Date;
  updatedAt?: Date;
}

export interface FamilySearchFilter {
  name?: string;
  description?: string;
  address?: string;
  visibility?: 'all' | 'public' | 'private' | 'shared';
  createdAtStart?: Date;
  createdAtEnd?: Date;
  updatedAtStart?: Date;
  updatedAtEnd?: Date;
}

export interface ConfirmDeleteDialogProps {
  modelValue: boolean;
  title: string;
  message: string;
}
