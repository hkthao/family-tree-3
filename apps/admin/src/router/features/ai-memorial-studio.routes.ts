import type { RouteRecordRaw } from 'vue-router';
import { AIMemorialStudioSelectionView, AIMemorialStudioMemberView, MemoryDetailPage, MemoryEditPage } from '@/views/ai-memorial-studio';

export const aiMemorialStudioRoutes: RouteRecordRaw[] = [
  {
    path: '/ai-memorial-studio',
    redirect: '/ai-memorial-studio/selection', // Explicit redirect to selection view
    meta: { breadcrumb: 'aiMemorialStudio.selection.title' },
  },
  {
    path: '/ai-memorial-studio/selection',
    name: 'AIMemorialStudioSelection',
    component: AIMemorialStudioSelectionView,
    meta: { breadcrumb: 'aiMemorialStudio.selection.title' },
  },
  {
    path: '/ai-memorial-studio/:memberId/:aiMemorialStudioType?',
    name: 'AIMemorialStudioMemberView',
    component: AIMemorialStudioMemberView,
    props: true,
    meta: { breadcrumb: 'memory.studio.title' },
    children: [
      {
        path: 'memories/:memoryId',
        name: 'MemoryDetail',
        component: MemoryDetailPage,
        props: true,
        meta: { breadcrumb: 'memory.detail.titleDefault' },
      },
      {
        path: 'memories/:memoryId/edit',
        name: 'MemoryEdit',
        component: MemoryEditPage,
        props: true,
        meta: { breadcrumb: 'memory.edit.title' },
      },
    ],
  },
];
