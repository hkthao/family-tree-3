import type { RouteRecordRaw } from 'vue-router';
import { DashboardView, FamilyTreeView, AddMemberView, MembersView, TimelineView, ProfileView, UserManagementView, RoleManagementView, FamilyManagementView } from '@/views';

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
    path: '/family/add',
    name: 'AddMember',
    component: AddMemberView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor'] },
  },
  {
    path: '/family/members',
    name: 'ManageMembers',
    component: MembersView,
    meta: { roles: ['Admin', 'FamilyManager', 'Editor'] },
  },
  {
    path: '/family/timeline',
    name: 'FamilyTimeline',
    component: TimelineView,
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
    path: '/family-management',
    name: 'FamilyManagement',
    component: FamilyManagementView,
    meta: { roles: ['Admin', 'FamilyManager'] },
  },
  // Add other routes here...
];