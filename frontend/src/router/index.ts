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
      path: '/',
      component: DashboardLayout,
      children: sidebarRoutes,
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