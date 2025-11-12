import { MainRouterView } from '@/views';
import {
  EventAddView,
  EventDetailView,
  EventEditView,
  EventListView,
} from '@/views/event';
import type { RouteRecordRaw } from 'vue-router';

export const eventRoutes: RouteRecordRaw[] = [
  {
    path: 'event',
    name: 'Event',
    component: MainRouterView,
    meta: { breadcrumb: 'event.list.title' },
    children: [
      {
        path: '',
        name: 'EventList',
        component: EventListView,
        meta: { breadcrumb: 'event.list.title' },
      },
      {
        path: 'add',
        name: 'AddEvent',
        component: EventAddView,
        meta: { breadcrumb: 'event.form.addTitle' },
      },
      {
        path: 'edit/:id',
        name: 'EditEvent',
        component: EventEditView,
        meta: { breadcrumb: 'event.form.editTitle' },
      },
      {
        path: 'detail/:id',
        name: 'EventDetail',
        component: EventDetailView,
        meta: { breadcrumb: 'event.detail.title' },
      },
    ],
  },
];
