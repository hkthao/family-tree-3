// apps/admin/src/router/features/user-push-token.routes.ts

import { MainRouterView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';
import UserPushTokenListView from '@/views/user-push-token/UserPushTokenListView.vue';

export const userTokenRoutes: Array<RouteRecordRaw> = [
  {
    path: '/user-push-tokens',
    name: 'UserTokenRoute',
    component: MainRouterView,
    meta: {
      requiresAuth: true,
      roles: ['Admin'],
      title: 'userToken.title',
      breadcrumb: 'userToken.title',
    },
    children: [{
      path: '',
      name: 'UserPushTokensList',
      component: UserPushTokenListView,
      meta: {
        title: 'userToken.title',
        breadcrumb: 'userPushToken.list.title',
      },
    }]
  },
];

export default userTokenRoutes;
