import type { RouteRecordRaw } from 'vue-router';
import { UserSettingsPage } from '@/views';

export const settingRoutes: RouteRecordRaw[] = [
  {
    path: 'settings',
    name: 'UserSettings',
    component: UserSettingsPage,
    meta: {
      breadcrumb: 'settings.title',
    },
  },
];