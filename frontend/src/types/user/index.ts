export interface User {
  id: string;
  externalId: string;
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

export interface UserProfile {
    id: string;
    externalId: string;
    email: string;
    name: string;
    avatar?: string;
}
