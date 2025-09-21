import type { User } from '@/components/layout/UserMenu.types';

export const mockUser: User = {
  id: 'a1b2c3d4-e5f6-7890-1234-567890abcde0',
  name: 'John Doe',
  roles: ['Admin'],
  avatarUrl: 'https://randomuser.me/api/portraits/men/85.jpg',
  online: true,
};