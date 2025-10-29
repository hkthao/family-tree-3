import { MainRouterView } from '@/views';
import { ChunkAdmin } from '@/views/admin';
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

    ],
  },
];