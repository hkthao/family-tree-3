import type { UserMenuItem } from '@/components/layout/UserMenu.types';

export const userMenuItems: UserMenuItem[] = [

  { key: 'settings', labelKey: 'userMenu.settings', icon: 'mdi-cog', to: '/settings' },

  { key: 'faq', labelKey: 'userMenu.faq', icon: 'mdi-help-circle', to: '/faq' },
];
