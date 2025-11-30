import type { RouteRecordRaw } from 'vue-router';

export const memberFaceRoutes: RouteRecordRaw[] = [
  {
    path: 'member-faces',
    name: 'MemberFaces',
    component: () => import('@/views/member-face/MemberFaceListView.vue'),
    meta: {
      requiresAuth: true,
      roles: ['Admin', 'Manager'], // Assuming these roles can manage member faces
      title: 'Member Faces',
    },
  },
];
