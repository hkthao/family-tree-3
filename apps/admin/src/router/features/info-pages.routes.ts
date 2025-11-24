import type { RouteRecordRaw } from 'vue-router';
import { ApplicationInfoPage, SupportLegalPage } from '@/views';

export const infoPagesRoutes: RouteRecordRaw[] = [
  {
    path: 'application-info',
    name: 'ApplicationInfo',
    component: ApplicationInfoPage,
    meta: { breadcrumb: 'menu.applicationInfo' },
  },
  {
    path: 'support-legal',
    name: 'SupportLegal',
    component: SupportLegalPage,
    meta: { breadcrumb: 'menu.supportAndLegal' },
  },
];
