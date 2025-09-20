export interface Family {
  id: number;
  name: string;
  description?: string;
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