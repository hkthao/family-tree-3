import { FamilyTreeView, ProfileView } from '@/views';
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
    path: '/profile',
    name: 'UserProfile',
    component: ProfileView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'profile.myProfile' },
  },
  {
    path: '/face/search',
    name: 'FaceSearch',
    component: () => import('@/views/face/FaceSearchView.vue'),
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], breadcrumb: 'search.face', icon: 'mdi-face-recognition' },
  },
];