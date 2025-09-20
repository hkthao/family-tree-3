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
        },
        {
          path: 'members',
          name: 'Members',
          component: () => import('@/views/members/MemberView.vue'),
        },
        {
          path: 'members/add',
          name: 'AddMember',
          component: () => import('@/views/members/MemberAddView.vue'),
        },
        {
          path: 'members/edit/:id',
          name: 'EditMember',
          component: () => import('@/views/members/MemberEditView.vue'),
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