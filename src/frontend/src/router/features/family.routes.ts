import { MainRouterView } from '@/views';
import {
  FamilyAddView,
  FamilyDetailView,
  FamilyEditView,
  FamilyListView,
  FamilyTreeView,
} from '@/views/family';
import type { RouteRecordRaw } from 'vue-router';

export const familyRoutes: RouteRecordRaw[] = [
  {
    path: 'family',
    name: 'FamilyManagement',
    component: MainRouterView,
    meta: { breadcrumb: 'family.management.title' },
    children: [
      {
        path: '',
        name: 'FamilyList',
        component: FamilyListView,
        meta: { breadcrumb: 'family.management.title' },
      },
      {
        path: 'add',
        name: 'AddFamily',
        component: FamilyAddView,
        meta: { breadcrumb: 'family.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'FamilyDetail',
        component: FamilyDetailView,
        meta: { breadcrumb: 'family.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditFamily',
        component: FamilyEditView,
        meta: { breadcrumb: 'family.form.editTitle' },
      },
      {
        path: 'tree',
        name: 'FamilyTree',
        component: FamilyTreeView,
        meta: { breadcrumb: 'family.tree.title' },
      },
    ],
  },
];
