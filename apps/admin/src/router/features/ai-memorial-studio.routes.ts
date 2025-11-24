import type { RouteRecordRaw } from 'vue-router';
import { AIMemorialStudioSelectionView } from '@/views';

export const aiMemorialStudioRoutes: RouteRecordRaw[] = [
  {
    path: '/ai-memorial-studio',
    name: 'AIMemorialStudioSelection',
    component: AIMemorialStudioSelectionView,
    meta: { breadcrumb: 'aiMemorialStudio.selection.title' },
  },
];
