import { FamilyTreeView, ProfileView, UserManagementView, RoleManagementView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';

export const sidebarRoutes: RouteRecordRaw[] = [
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
    path: '/admin/users',
    name: 'UserManagement',
    component: UserManagementView,
    meta: { roles: ['Admin'], breadcrumb: 'admin.users' },
  },
  {
    path: '/admin/roles',
    name: 'RoleManagement',
    component: RoleManagementView,
    meta: { roles: ['Admin'], breadcrumb: 'admin.roles' },
  },
  // Add other routes here...
];