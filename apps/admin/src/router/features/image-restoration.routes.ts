import type { RouteRecordRaw } from 'vue-router';
import { MainRouterView } from '@/views';
import ImageRestorationView from '@/views/image-restoration/ImageRestorationView.vue';

export const imageRestorationRoutes: RouteRecordRaw[] = [
  {
    path: 'image-restoration',
    name: 'ImageRestoration',
    component: MainRouterView,
    meta: { breadcrumb: 'menu.imageRestoration', requiresAuth: true, roles: ['Admin'] },
    children: [
      {
        path: '',
        name: 'ImageRestorationView',
        component: ImageRestorationView,
        meta: { breadcrumb: 'menu.imageRestoration' },
      },
    ],
  },
];
