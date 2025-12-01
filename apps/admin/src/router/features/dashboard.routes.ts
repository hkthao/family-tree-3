import type { RouteRecordRaw } from 'vue-router';
import { DashboardView } from '@/views';

export const dashboardRoutes: RouteRecordRaw[] = [
  {
    path: 'dashboard',
    name: 'Dashboard',
    component: DashboardView,
    meta: { breadcrumb: 'dashboard.title' },
  },
];
