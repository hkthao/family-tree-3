import { MainRouterView } from '@/views';
import {
  MemberAddView,
  MemberBiographyView,
  MemberDetailView,
  MemberEditView,
  MemberListView,
} from '@/views/member';
import type { RouteRecordRaw } from 'vue-router';

export const memberRoutes: RouteRecordRaw[] = [
  {
    path: 'member',
    name: 'Member',
    component: MainRouterView,
    meta: { breadcrumb: 'member.list.title' },
    children: [
      {
        path: '',
        name: 'MemberList',
        component: MemberListView,
        meta: { breadcrumb: 'member.list.title' },
      },
      {
        path: 'add',
        name: 'AddMember',
        component: MemberAddView,
        meta: { breadcrumb: 'member.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'MemberDetail',
        component: MemberDetailView,
        meta: { breadcrumb: 'member.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditMember',
        component: MemberEditView,
        meta: { breadcrumb: 'member.form.editTitle' },
      },
      {
        path: 'biography/:memberId',
        name: 'MemberBiography',
        component: MemberBiographyView,
        meta: { breadcrumb: 'aiBiography.generator.title' },
      },
    ],
  },
];