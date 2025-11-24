import type { RouteRecordRaw } from 'vue-router';
import { MemberMemoriesView } from '@/views';

export const memoryRoutes: RouteRecordRaw[] = [
  {
    path: 'members/:memberId/memories',
    name: 'MemberMemories',
    component: MemberMemoriesView,
    meta: { breadcrumb: 'memory.studio.title' },
  },
];
