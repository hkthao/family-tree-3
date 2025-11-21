import { createRouter, createWebHistory } from 'vue-router';
import { MainLayout } from '@/layouts';

import { canAccessMenu } from '@/utils/menuPermissions';
import { useAuthStore } from '@/stores';
import { useAuthService } from '@/services/auth/authService';
import type { AppState } from '@/types';

// Import feature routes
import { memberRoutes } from './features/member.routes';
import { familyRoutes } from './features/family.routes';
import { faceRoutes } from './features/face.routes';
import { settingRoutes } from './features/setting.routes';
import { donateRoutes } from './features/donate.routes';

// Import all pages from the views index
import {
  ApplicationInfoPage,
  SupportLegalPage,
  LogoutView,
  DashboardView,
  NotFoundView,
  NLEditorView,
} from '@/views';

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
          component: DashboardView,
          meta: { breadcrumb: 'dashboard.title' },
        },
        ...memberRoutes,
        ...familyRoutes,
        ...faceRoutes,
        ...settingRoutes,
        ...donateRoutes,
        {
          path: 'nl-editor',
          name: 'NLEditor',
          component: NLEditorView,
          meta: { breadcrumb: 'naturalLanguage.editor.title' },
        },
        {
          path: 'application-info',
          name: 'ApplicationInfo',
          component: ApplicationInfoPage,
          meta: { breadcrumb: 'menu.applicationInfo' },
        },
        {
          path: 'support-legal',
          name: 'SupportLegal',
          component: SupportLegalPage,
          meta: { breadcrumb: 'menu.supportAndLegal' },
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
      path: '/public/family-tree/:familyId/:rootId?',
      name: 'PublicFamilyTreeViewer',
      component: () => import('@/views/PublicFamilyTreeViewer.vue'),
      meta: { requiresAuth: false }, // Public route does not require authentication
    },
    {
      path: '/public/support-legal',
      name: 'PublicSupportLegal',
      component: SupportLegalPage,
      meta: { requiresAuth: false }, // Public route does not require authentication
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: NotFoundView,
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

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth) && !to.matched.some(record => record.meta.requiresAuth === false);
  const requiredRoles = to.meta.roles as string[];

  if (requiresAuth && !authStore.isAuthenticated) {
    // If the route requires authentication and the user is not authenticated, redirect to login
    await authService.login({ appState: { target: to.fullPath } });
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
