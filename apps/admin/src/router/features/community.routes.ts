import { MainRouterView } from '@/views';
import PublicFamilyListView from '@/views/family/PublicFamilyListView.vue';

export const communityRoutes = [
  {
    path: '/community/families',
    name: 'community',
    component: MainRouterView,
    children: [
      {
      path: '',
      name:"PublicFamilyList",
      component: PublicFamilyListView,
      meta: {
        requiresAuth: true,
        title: 'menu.community', // Use i18n key for title
        icon: 'mdi-account-group', // Add icon for the route
        breadcrumb: 'menu.community', // Add breadcrumb for the route
      },
    }
    ]
  },
];
