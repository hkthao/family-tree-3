import type { RouteRecordRaw } from 'vue-router';
import { MainRouterView } from '@/views';
import ImageRestorationJobListView from '@/views/image-restoration-job/ImageRestorationJobListView.vue';
import ImageRestorationJobAddView from '@/views/image-restoration-job/ImageRestorationJobAddView.vue';
import ImageRestorationJobEditView from '@/views/image-restoration-job/ImageRestorationJobEditView.vue';
import ImageRestorationJobDetailView from '@/views/image-restoration-job/ImageRestorationJobDetailView.vue';

export const imageRestorationRoutes: RouteRecordRaw[] = [
  {
    path: 'image-restoration-jobs', // Changed path to plural
    name: 'ImageRestorationJobs', // Changed name to plural
    component: MainRouterView,
    meta: { breadcrumb: 'menu.imageRestorationJobs', requiresAuth: true, roles: ['Admin'] }, // Updated breadcrumb
    children: [
      {
        path: '',
        name: 'ImageRestorationJobList',
        component: ImageRestorationJobListView,
        meta: { breadcrumb: 'menu.imageRestorationJobs' },
      },
      {
        path: 'add',
        name: 'ImageRestorationJobAdd',
        component: ImageRestorationJobAddView,
        meta: { breadcrumb: 'menu.imageRestorationJobAdd' },
      },
      {
        path: ':id/edit', // Using :id for specific job
        name: 'ImageRestorationJobEdit',
        component: ImageRestorationJobEditView,
        meta: { breadcrumb: 'menu.imageRestorationJobEdit' },
        props: true,
      },
      {
        path: ':id', // Using :id for specific job
        name: 'ImageRestorationJobDetail',
        component: ImageRestorationJobDetailView,
        meta: { breadcrumb: 'menu.imageRestorationJobDetail' },
        props: true,
      },
    ],
  },
];
