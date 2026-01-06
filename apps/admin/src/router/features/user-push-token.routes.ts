// apps/admin/src/router/features/user-push-token.routes.ts

import type { RouteRecordRaw } from 'vue-router';
import UserPushTokenListView from '@/views/user-push-token/UserPushTokenListView.vue';

const userRoutes: RouteRecordRaw[] = [
  {
    path: '/users/:userId/push-tokens',
    name: 'UserPushTokensList',
    component: UserPushTokenListView,
    props: true, // Pass route params as props to the component
    meta: {
      requiresAuth: true,
      hasPermission: ['ADMINISTRATOR'], // Restrict access to administrators
      title: 'User Push Tokens',
    },
  },
];

export default userRoutes;
