import type { RouteRecordRaw } from 'vue-router';

export const donateRoutes: Array<RouteRecordRaw> = [
  {
    path: '/donate',
    name: 'Donate',
    component: () => import('@/views/DonateView.vue'),
    meta: {
      requiresAuth: true,
      layout: 'MainLayout',
      title: 'Donate',
    },
  },
];

