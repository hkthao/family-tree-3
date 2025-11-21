import { MainRouterView, FamilyDictAddView, FamilyDictDetailView, FamilyDictEditView, FamilyDictListView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';

export const familyDictRoutes: RouteRecordRaw[] = [
  {
    path: 'family-dict',
    name: 'FamilyDict',
    component: MainRouterView,
    meta: { breadcrumb: 'familyDict.list.title', requiresAuth: true, roles: ['Admin'] },
    children: [
      {
        path: '',
        name: 'FamilyDictList',
        component: FamilyDictListView,
        meta: { breadcrumb: 'familyDict.list.title' },
      },
      {
        path: 'add',
        name: 'AddFamilyDict',
        component: FamilyDictAddView,
        meta: { breadcrumb: 'familyDict.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'FamilyDictDetail',
        component: FamilyDictDetailView,
        meta: { breadcrumb: 'familyDict.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditFamilyDict',
        component: FamilyDictEditView,
        meta: { breadcrumb: 'familyDict.form.editTitle' },
      },
    ],
  },
];
