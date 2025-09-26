import { createRouter, createWebHistory } from 'vue-router';
import DashboardLayout from '@/layouts/dashboard/DashboardLayout.vue';
import { sidebarRoutes } from './sidebar-routes';
import { canAccessMenu } from '@/utils/menu-permissions';

// Mock user store
const useUserStore = () => ({
  roles: ['FamilyManager'], // or ['Admin'], ['Viewer'] etc.
});

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
      component: DashboardLayout,
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
          ],
        },
        {
          path: 'events',
          name: 'Events',
          component: () => import('@/views/events/EventListView.vue'),
          meta: { breadcrumb: 'event.list.title' },
          children: [
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
          ],
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

router.beforeEach((to, from, next) => {
  const userStore = useUserStore();
  const requiredRoles = to.meta.roles as string[];

  if (requiredRoles) {
    if (canAccessMenu(userStore.roles, requiredRoles)) {
      next();
    } else {
      next({ name: 'Dashboard' }); // Or a dedicated 'Access Denied' page
    }
  }
  else {
    next(); // No roles required
  }
});

export default router;