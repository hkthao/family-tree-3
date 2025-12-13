// apps/admin/src/router/features/prompt.routes.ts

import { MainRouterView } from '@/views';
import { PromptAddView, PromptDetailView, PromptEditView, PromptListView } from '@/views/prompt';
import type { RouteRecordRaw } from 'vue-router';

export const promptRoutes: RouteRecordRaw[] = [
  {
    path: 'prompts',
    name: 'Prompt',
    component: MainRouterView,
    meta: { breadcrumb: 'prompt.list.title', requiresAuth: true, roles: ['Admin'] },
    children: [
      {
        path: '',
        name: 'PromptList',
        component: PromptListView,
        meta: { breadcrumb: 'prompt.list.title' },
      },
      {
        path: 'add',
        name: 'AddPrompt',
        component: PromptAddView,
        meta: { breadcrumb: 'prompt.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'PromptDetail',
        component: PromptDetailView,
        meta: { breadcrumb: 'prompt.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditPrompt',
        component: PromptEditView,
        meta: { breadcrumb: 'prompt.form.editTitle' },
      },
    ],
  },
];
