import type { RouteRecordRaw } from 'vue-router';
import { LogoutView, NotFoundView } from '@/views';

export const miscRoutes: RouteRecordRaw[] = [
  {
    path: '/logout',
    name: 'Logout',
    component: LogoutView,
    meta: { requiresAuth: false }, // Logout page does not require authentication
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFoundView,
  },
];
