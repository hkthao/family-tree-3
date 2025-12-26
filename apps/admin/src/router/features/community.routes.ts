import PublicFamilyListView from '@/views/community/PublicFamilyListView.vue';

export const communityRoutes = [
  {
    path: '/community/families',
    name: 'PublicFamilyList',
    component: PublicFamilyListView,
    meta: {
      requiresAuth: true,
      title: 'menu.community', // Use i18n key for title
    },
  },
];
