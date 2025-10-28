import { MainRouterView } from '@/views';
import { ChunkAdmin, ConfigView as SystemConfig } from '@/views/admin';
export const adminRoutes: RouteRecordRaw[] = [
  {
    path: 'admin',
    name: 'Admin',
    component: MainRouterView,
    meta: { breadcrumb: 'admin.title', requiresAuth: true, roles: ['Admin'] },
    children: [
      {
        path: 'chunk',
        name: 'ChunkAdmin',
        component: ChunkAdmin,
        meta: { breadcrumb: 'admin.chunks.title', requiresAuth: true, roles: ['Admin'] },
      },
      {
        path: 'config',
        name: 'SystemConfig',
        component: SystemConfig,
        meta: { breadcrumb: 'admin.config.title', requiresAuth: true, roles: ['Admin'] },
      },

    ],
  },
];