import { MainRouterView } from '@/views';
import FaceRecognitionView from '@/views/face/FaceRecognitionView.vue';
import FaceSearchView from '@/views/face/FaceSearchView.vue';
import type { RouteRecordRaw } from 'vue-router';

export const faceRoutes: RouteRecordRaw[] = [
  {
    path: 'face',
    name: 'Face',
    component: MainRouterView,
    meta: { breadcrumb: 'face.title', requiresAuth: true },
    children: [
      {
        path: 'search',
        name: 'FaceSearch',
        component: FaceSearchView,
        meta: { breadcrumb: 'face.search.title', requiresAuth: true },
      },
    ],
  },
];