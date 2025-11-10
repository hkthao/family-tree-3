import { createRouter, createWebHistory } from 'vue-router';
import { MainLayout } from '@/layouts';
import LogoutView from '@/views/auth/LogoutView.vue'; // Import LogoutView

import { canAccessMenu } from '@/utils/menuPermissions';
import { useAuthStore } from '@/stores';
import { useAuthService } from '@/services/auth/authService';
import type { AppState } from '@/types';

// Import feature routes
import { memberRoutes } from './features/member.routes';
import { familyRoutes } from './features/family.routes';
import { faceRoutes } from './features/face.routes';
import { settingRoutes } from './features/setting.routes';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
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
        ...memberRoutes,
        ...familyRoutes,
        ...faceRoutes,
        ...settingRoutes,
        {
          path: 'nl-confirmation',
          name: 'NLConfirmation',
          component: () => import('@/views/natural-language/NLResultConfirmationView.vue'),
          meta: { breadcrumb: 'naturalLanguage.confirmation.title' },
        },
      ],
    },
    {
      path: '/logout',
      name: 'Logout',
      component: LogoutView,
      meta: { requiresAuth: false }, // Logout page does not require authentication
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
  const authService = useAuthService();

  // Handle Auth0 redirect callback
  if (to.name === 'Auth0Callback' && (to.query.code || to.query.state)) {
    try {
      const appState = (await authService.handleRedirectCallback()) as AppState;
      // Redirect to the original target or dashboard
      const targetPath = appState?.target || '/';
      next(targetPath);
      return;
    } catch (error) {
      console.error('Error handling Auth0 redirect callback:', error);
      // Redirect to login or an error page
      next('/');
      return;
    }
  }

  await authStore.initAuth(); // Ensure auth state is initialized

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const requiredRoles = to.meta.roles as string[];

  if (requiresAuth && !authStore.isAuthenticated) {
    // Initiate Auth0 login redirect
    await authService.login({ appState: { target: to.fullPath } });
    return; // Prevent further navigation
  } else if (requiredRoles && !canAccessMenu(authStore.user?.roles || [], requiredRoles)) {
    next({ name: 'Dashboard' }); // Or a dedicated 'Access Denied' page
  } else {
    next();
  }
});

export default router;
