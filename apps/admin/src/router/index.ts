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

// Import all pages from the views index
import {
  AboutPage,
  VersionPage,
  HelpPage,
  TermsPage,
  PrivacyPage,
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
        {
          path: 'nl-editor',
          name: 'NLEditor',
          component: NLEditorView,
          meta: { breadcrumb: 'naturalLanguage.editor.title' },
        },
        {
          path: 'about',
          name: 'About',
          component: AboutPage,
          meta: { breadcrumb: 'about.title' },
        },
        {
          path: 'version',
          name: 'Version',
          component: VersionPage,
          meta: { breadcrumb: 'version.title' },
        },
        {
          path: 'help',
          name: 'Help',
          component: HelpPage,
          meta: { breadcrumb: 'help.title' },
        },

        {
          path: 'terms',
          name: 'Terms',
          component: TermsPage,
          meta: { breadcrumb: 'terms.title' },
        },
        {
          path: 'privacy',
          name: 'Privacy',
          component: PrivacyPage,
          meta: { breadcrumb: 'privacy.title' },
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
