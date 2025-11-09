import type { RouteRecordRaw } from 'vue-router';

export const settingRoutes: RouteRecordRaw[] = [
  {
    path: 'settings',
    name: 'UserSettings',
    component: () => import('@/views/settings/UserSettingsPage.vue'),
    meta: {
      breadcrumb: 'settings.title',
    },
  },
];