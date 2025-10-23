import { FamilyTreeView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';

export const sidebarRoutes: RouteRecordRaw[] = [
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/dashboard/DashboardView.vue'),
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'dashboard.title', icon: 'mdi-view-dashboard' },
  },
  {
    path: '/family/tree',
    name: 'FamilyTree',
    component: FamilyTreeView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'family.tree.title' },
  },
  {
    path: '/face',
    name: 'Face',
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'face.sidebar.title', icon: 'mdi-face-recognition' },
    children: [
      {
        path: 'recognition',
        name: 'FaceRecognition',
        component: () => import('@/views/face/FaceRecognitionView.vue'),
        meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'face.recognition.title' },
      },
      {
        path: 'search',
        name: 'FaceSearch',
        component: () => import('@/views/face/FaceSearchView.vue'),
        meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'search.face' },
      },
    ],
  },
];