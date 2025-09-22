import { DashboardView, FamilyTreeView, ProfileView, UserManagementView, RoleManagementView, FamilyListView, EventListView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';

export const sidebarRoutes: RouteRecordRaw[] = [
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'] },
  },
  {
    path: '/family/tree',
    name: 'FamilyTree',
    component: FamilyTreeView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'] },
  },
  {
    path: '/events',
    name: 'Events',
    component: EventListView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'] },
  },
  {
    path: '/profile',
    name: 'UserProfile',
    component: ProfileView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'] },
  },
  {
    path: '/admin/users',
    name: 'UserManagement',
    component: UserManagementView,
    meta: { roles: ['Admin'] },
  },
  {
    path: '/admin/roles',
    name: 'RoleManagement',
    component: RoleManagementView,
    meta: { roles: ['Admin'] },
  },
  {
    path: '/family',
    name: 'FamilyManagement',
    component: FamilyListView,
    meta: { roles: ['Admin', 'FamilyManager'] },
  },
  // Add other routes here...
];