import { createRouter, createWebHistory } from 'vue-router';
import DashboardView from '../views/DashboardView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
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
  ],
});

export default router;
