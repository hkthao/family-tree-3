export interface User {
  id: string;
  name: string;
  email: string;
  roles?: string[];
  avatar?: string;
  online?: boolean;
}

export interface UserMenuItem {
  key: string;
  labelKey: string;
  icon: string;
  to?: string;
}
