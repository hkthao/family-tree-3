import type { RouteRecordRaw } from 'vue-router';
import { MemberMemoriesView, MemoryDetailPage, MemoryEditPage } from '@/views'; // Import new views

export const memoryRoutes: RouteRecordRaw[] = [
  {
    path: 'members/:memberId/memories',
    name: 'MemberMemories',
    component: MemberMemoriesView,
    meta: { breadcrumb: 'memory.studio.title' },
  },
  {
    path: 'memories/:memoryId',
    name: 'MemoryDetail',
    component: MemoryDetailPage,
    meta: { breadcrumb: 'memory.detail.titleDefault' },
  },
  {
    path: 'memories/:memoryId/edit',
    name: 'MemoryEdit',
    component: MemoryEditPage,
    meta: { breadcrumb: 'memory.edit.title' },
  },
];
