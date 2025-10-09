export interface User {
  id: string;
  auth0UserId: string;
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
    auth0UserId: string;
    email: string;
    name: string;
    avatar?: string;
}
