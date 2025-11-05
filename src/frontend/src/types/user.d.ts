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
    userId?: string;
    email: string;
    name: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    avatar?: string;
    roles?: string[];
}