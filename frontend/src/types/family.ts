export interface Family {
  id: string;
  name: string;
  description?: string;
  address?: string;
  avatarUrl?: string;
  visibility: 'Private' | 'Public';
}

export interface Props {
  family?: Family;
}

export interface ConfirmDeleteDialogProps {
  modelValue: boolean;
  title?: string;
  message?: string;
}

export interface FamilyFilter {
  fullName?: string;
  visibility?: 'All' | 'Private' | 'Public';
}