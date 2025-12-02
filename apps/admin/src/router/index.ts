import { createRouter, createWebHistory } from 'vue-router';
import { MainLayout } from '@/layouts';

import { canAccessMenu } from '@/utils/menuPermissions';
import { useAuthStore } from '@/stores';
import { useAuthService } from '@/services/auth/authService';
import type { AppState } from '@/types';
import { getEnvVariable } from '@/utils/api.util'; // Import getEnvVariable

// Import feature routes
import { familyDictRoutes } from './features/family-dict.routes';
import { memberRoutes } from './features/member.routes';
import { familyRoutes } from './features/family.routes';
import { faceRoutes } from './features/face.routes';
import { settingRoutes } from './features/setting.routes';
import { donateRoutes } from './features/donate.routes';
import { eventRoutes } from './features/event.routes';
import { dashboardRoutes } from './features/dashboard.routes'; // New

import { nlEditorRoutes } from './features/nl-editor.routes'; // New
import { infoPagesRoutes } from './features/info-pages.routes'; // New
import { publicRoutes } from './features/public.routes'; // New
import { miscRoutes } from './features/misc.routes'; // New
import { memberStoryRoutes } from './features/member-story.routes'; // Updated
import { chatRoutes } from './features/chat.routes'; // New

const router = createRouter({
  history: createWebHistory(getEnvVariable('BASE_URL')),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
      component: MainLayout,
      meta: { requiresAuth: true }, // Add requiresAuth meta
      children: [
        ...dashboardRoutes, // Replaced
        ...memberRoutes,
        ...memberStoryRoutes, // Updated
        ...familyDictRoutes,
        ...familyRoutes,
        ...faceRoutes,
        ...settingRoutes,
        ...donateRoutes,
        ...eventRoutes,
        ...nlEditorRoutes, // Replaced
        ...infoPagesRoutes, // Replaced
        ...chatRoutes, // New
      ],
    },
    ...publicRoutes, // Replaced
    ...miscRoutes, // Replaced
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

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth) && !to.matched.some(record => record.meta.requiresAuth === false);
  const requiredRoles = to.meta.roles as string[];

  if (requiresAuth && !authStore.isAuthenticated) {
    // If the route requires authentication and the user is not authenticated, redirect to login
    await authService.login({ appState: { target: to.fullPath } });
    next(false); // Ngăn chặn điều hướng tiếp theo vì authService.login() sẽ xử lý chuyển hướng.
    return;
  } else if (requiredRoles && !canAccessMenu(authStore.user?.roles || [], requiredRoles)) {
    // If the route requires specific roles and the user doesn't have them
    next({ name: 'Dashboard' }); // Or a dedicated 'Access Denied' page
  } else {
    // Proceed to the route
    next();
  }
});

export default router;
