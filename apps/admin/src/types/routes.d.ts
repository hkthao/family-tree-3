import type { RouteRecordRaw } from 'vue-router';

// Extend RouteRecordRaw to include custom meta fields
export interface AppRoute extends RouteRecordRaw {
  meta?: {
    requiresAuth?: boolean;
    roles?: string[];
    title?: string; // For i18n key for page title
    icon?: string; // For menu icon
    hidden?: boolean; // To hide from menu
  };
}
