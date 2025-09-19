import type { User } from '@/components/layout/UserMenu.types';

export const mockUser: User = {
  id: 'u1',
  name: 'John Doe',
  roles: ['Admin'],
  avatarUrl: 'https://randomuser.me/api/portraits/men/85.jpg',
  online: true,
};
