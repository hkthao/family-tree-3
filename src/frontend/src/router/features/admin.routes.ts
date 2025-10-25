import { MainRouterView } from '@/views';
import { ChunkAdmin, ConfigView as SystemConfig } from '@/views/admin';
import {
  NotificationTemplateAddView,
  NotificationTemplateEditView,
  NotificationTemplateListView,
} from '@/views/notification-template';
import type { RouteRecordRaw } from 'vue-router';

export const adminRoutes: RouteRecordRaw[] = [
  {
    path: 'admin',
    name: 'Admin',
    component: MainRouterView,
    meta: { breadcrumb: 'admin.title', requiresAuth: true, roles: ['Admin'] },
    children: [
      {
        path: 'chunk',
        name: 'ChunkAdmin',
        component: ChunkAdmin,
        meta: { breadcrumb: 'admin.chunks.title', requiresAuth: true, roles: ['Admin'] },
      },
      {
        path: 'config',
        name: 'SystemConfig',
        component: SystemConfig,
        meta: { breadcrumb: 'admin.config.title', requiresAuth: true, roles: ['Admin'] },
      },
      {
        path: 'notification-template',
        name: 'NotificationTemplate',
        component: MainRouterView,
        meta: { breadcrumb: 'admin.notificationTemplates.title', requiresAuth: true, roles: ['Admin'] },
        children: [
          {
            path: '',
            name: 'NotificationTemplateList',
            component: NotificationTemplateListView,
            meta: { breadcrumb: 'admin.notificationTemplates.list.title', requiresAuth: true, roles: ['Admin'] },
          },
          {
            path: 'add',
            name: 'AddNotificationTemplate',
            component: NotificationTemplateAddView,
            meta: { breadcrumb: 'admin.notificationTemplates.form.addTitle', requiresAuth: true, roles: ['Admin'] },
          },
          {
            path: 'edit/:id',
            name: 'EditNotificationTemplate',
            component: NotificationTemplateEditView,
            props: true,
            meta: { breadcrumb: 'admin.notificationTemplates.form.editTitle', requiresAuth: true, roles: ['Admin'] },
          },
        ],
      },
    ],
  },
];