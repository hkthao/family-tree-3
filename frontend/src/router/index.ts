import { createRouter, createWebHistory } from 'vue-router';
import Dashboard from '../views/Dashboard.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: Dashboard,
    },
    {
      path: '/families',
      name: 'families',
      component: () => import('../views/FamiliesView.vue'),
    },
    {
      path: '/members',
      name: 'members',
      component: () => import('../views/MembersView.vue'),
    },
    {
      path: '/relationships',
      name: 'relationships',
      component: () => import('../views/RelationshipsView.vue'),
    },
    {
      path: '/tree',
      name: 'tree',
      component: () => import('../views/FamilyTreeView.vue'),
    },
    {
      path: '/family-management',
      name: 'FamilyManagement',
      component: () => import('../views/FamilyManagement.vue'),
    },
    {
      path: '/member-detail/:id',
      name: 'MemberDetail',
      component: () => import('@/components/MemberDetail.vue'),
      props: true,
    },
  ],
});

export default router;
