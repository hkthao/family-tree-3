import { MainRouterView, RelationshipAddView, RelationshipDetailView, RelationshipEditView, RelationshipListView } from '@/views';
import type { RouteRecordRaw } from 'vue-router';

export const relationshipRoutes: RouteRecordRaw[] = [
  {
    path: 'relationship',
    name: 'Relationship',
    component: MainRouterView,
    meta: { breadcrumb: 'relationship.list.title' },
    children: [
      {
        path: '',
        name: 'RelationshipList',
        component: RelationshipListView,
        meta: { breadcrumb: 'relationship.list.title' },
      },
      {
        path: 'add',
        name: 'AddRelationship',
        component: RelationshipAddView,
        meta: { breadcrumb: 'relationship.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'RelationshipDetail',
        component: RelationshipDetailView,
        meta: { breadcrumb: 'relationship.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditRelationship',
        component: RelationshipEditView,
        props: true,
        meta: { breadcrumb: 'relationship.form.editTitle' },
      },
    ],
  },
];
