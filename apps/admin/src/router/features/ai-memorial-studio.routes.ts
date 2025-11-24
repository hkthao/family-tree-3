import type { RouteRecordRaw } from 'vue-router';
import MemoryListView from '@/views/ai-memorial-studio/MemoryListView.vue';
import AIMemorialStudioMemberView from '@/views/ai-memorial-studio/AIMemorialStudioMemberView.vue';
import MemoryDetailPage from '@/views/ai-memorial-studio/MemoryDetailPage.vue';
import MemoryEditPage from '@/views/ai-memorial-studio/MemoryEditPage.vue';

export const aiMemorialStudioRoutes: RouteRecordRaw[] = [
  {
    path: '/ai-memorial-studio',
    redirect: '/ai-memorial-studio/list', // Redirect to the memory list
    meta: { breadcrumb: 'memory.list.title' },
  },
  {
    path: '/ai-memorial-studio/list',
    name: 'MemoryList',
    component: MemoryListView,
    meta: { breadcrumb: 'memory.list.title' },
  },
  {
    path: '/ai-memorial-studio/:memberId/:aiMemorialStudioType?',
    name: 'AIMemorialStudioMemberView',
    component: AIMemorialStudioMemberView,
    props: true,
    meta: { breadcrumb: 'memory.studio.title' },
  },
];
