import RelationshipDetectionView from '@/views/relationship-detection/RelationshipDetectionView.vue';

export const relationshipDetectionRoutes = [
  {
    path: 'relationship-detection', // Relative path for nested route
    name: 'RelationshipDetection',
    component: RelationshipDetectionView, // No need for defineAsyncComponent for small views
    meta: {
      requiresAuth: true,
      roles: ['Admin', 'Manager'], // Adjust roles as needed
      breadcrumb: 'relationshipDetection.title',
    },
  },
];
