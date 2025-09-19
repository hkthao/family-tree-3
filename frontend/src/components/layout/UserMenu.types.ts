export interface User {
  id: string;
  name: string;
  roles: string[];
  avatarUrl?: string;
  online?: boolean;
}

export interface UserMenuItem {
  key: string;
  labelKey: string;
  icon: string;
  to?: string;
}
