import { MainRouterView, MemberAddView, MemberDetailTabsView, MemberEditView, MemberListView } from '@/views';
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
        component: MemberDetailTabsView,
        meta: { breadcrumb: 'member.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditMember',
        component: MemberEditView,
        meta: { breadcrumb: 'member.form.editTitle' },
      },

      {
        path: 'faces',
        name: 'MemberFaces',
        component: () => import('@/views/member-face/MemberFaceListView.vue'),
        meta: {
          breadcrumb: 'memberFace.list.title',
        },
      },
    ],
  },
];