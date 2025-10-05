import { createRouter, createWebHistory } from 'vue-router';
import { MainLayout } from '@/layouts';
import { sidebarRoutes } from './sidebar-routes';
import { canAccessMenu } from '@/utils/menu-permissions';
import { useAuthStore } from '@/stores/auth.store';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/auth/LoginView.vue'),
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('@/views/auth/RegisterView.vue'),
    },
    {
      path: '/',
      redirect: '/dashboard',
      component: MainLayout,
      meta: { requiresAuth: true }, // Add requiresAuth meta
      children: [
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: () => import('@/views/dashboard/DashboardView.vue'),
          meta: { breadcrumb: 'dashboard.title' },
        },
        {
          path: 'members',
          name: 'Members',
          component: () => import('@/views/members/MemberRouterView.vue'),
          meta: { breadcrumb: 'member.list.title' },
          children: [
            {
              path: '',
              name: 'MemberList',
              component: () => import('@/views/members/MemberListView.vue'),
              meta: { breadcrumb: 'member.list.title' },
            },
            {
              path: 'add',
              name: 'AddMember',
              component: () => import('@/views/members/MemberAddView.vue'),
              meta: { breadcrumb: 'member.form.addTitle' },
            },
            {
              path: 'detail/:id',
              name: 'MemberDetail',
              component: () => import('@/views/members/MemberDetailView.vue'),
              meta: { breadcrumb: 'member.detail.title' },
            },
            {
              path: 'edit/:id',
              name: 'EditMember',
              component: () => import('@/views/members/MemberEditView.vue'),
              meta: { breadcrumb: 'member.form.editTitle' },
            },
          ],
        },
        {
          path: 'family',
          name: 'FamilyManagement',
          component: () => import('@/views/family/FamilyRouterView.vue'),
          meta: { breadcrumb: 'family.management.title' },
          children: [
            {
              path: '',
              name: 'FamilyList',
              component: () => import('@/views/family/FamilyListView.vue'),
              meta: { breadcrumb: 'family.management.title' },
            },
            {
              path: 'add',
              name: 'AddFamily',
              component: () => import('@/views/family/FamilyAddView.vue'),
              meta: { breadcrumb: 'family.form.addTitle' },
            },
            {
              path: 'detail/:id',
              name: 'FamilyDetail',
              component: () => import('@/views/family/FamilyDetailView.vue'),
              meta: { breadcrumb: 'family.detail.title' },
            },
            {
              path: 'edit/:id',
              name: 'EditFamily',
              component: () => import('@/views/family/FamilyEditView.vue'),
              meta: { breadcrumb: 'family.form.editTitle' },
            },
            {
              path: 'tree',
              name: 'FamilyTree',
              component: () => import('@/views/family/FamilyTreeView.vue'),
              meta: { breadcrumb: 'family.tree.title' },
            },
          ],
        },
        {
          path: 'events',
          name: 'Events',
          component: () => import('@/views/events/EventRouterView.vue'),
          meta: { breadcrumb: 'event.list.title' },
          children: [
            {
              path: '',
              name: 'EventList',
              component: () => import('@/views/events/EventListView.vue'),
              meta: { breadcrumb: 'event.list.title' },
            },
            {
              path: 'add',
              name: 'AddEvent',
              component: () => import('@/views/events/EventAddView.vue'),
              meta: { breadcrumb: 'event.form.addTitle' },
            },
            {
              path: 'edit/:id',
              name: 'EditEvent',
              component: () => import('@/views/events/EventEditView.vue'),
              meta: { breadcrumb: 'event.form.editTitle' },
            },
            {
              path: 'detail/:id',
              name: 'EventDetail',
              component: () => import('@/views/events/EventDetailView.vue'),
              meta: { breadcrumb: 'event.detail.title' },
            },
          ],
        },
        {
          path: 'relationships',
          name: 'Relationships',
          component: () => import('@/views/relationships/RelationshipRouterView.vue'),
          meta: { breadcrumb: 'relationship.list.title' },
          children: [
            {
              path: '',
              name: 'RelationshipList',
              component: () => import('@/views/relationships/RelationshipListView.vue'),
              meta: { breadcrumb: 'relationship.list.title' },
            },
            {
              path: 'add',
              name: 'AddRelationship',
              component: () => import('@/views/relationships/RelationshipAddView.vue'),
              meta: { breadcrumb: 'relationship.form.addTitle' },
            },
            {
              path: 'detail/:id',
              name: 'RelationshipDetail',
              component: () => import('@/views/relationships/RelationshipDetailView.vue'),
              meta: { breadcrumb: 'relationship.detail.title' },
            },
            {
              path: 'edit/:id',
              name: 'EditRelationship',
              component: () => import('@/views/relationships/RelationshipEditView.vue'),
              meta: { breadcrumb: 'relationship.form.editTitle' },
            },
          ],
        },
        {
          path: 'settings',
          name: 'UserSettings',
          component: () => import('@/views/settings/UserSettingsPage.vue'),
          meta: { breadcrumb: 'userSettings.title', roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'] },
        },
        ...sidebarRoutes,
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: () => import('@/views/misc/NotFoundView.vue'),
    },
  ],
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  await authStore.initAuth(); // Ensure auth state is initialized

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const requiredRoles = to.meta.roles as string[];

  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'Login' });
  } else if (requiredRoles && !canAccessMenu(authStore.user?.roles || [], requiredRoles)) {
    next({ name: 'Dashboard' }); // Or a dedicated 'Access Denied' page
  } else {
    next();
  }
});

export default router;