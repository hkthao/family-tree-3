import type { RouteRecordRaw } from 'vue-router';
import MemberStoryListView from '@/views/member-story-studio/MemberStoryListView.vue'; // Updated

export const memberStoryStudioRoutes: RouteRecordRaw[] = [ // Updated
  {
    path: '/member-story-studio', // Updated
    redirect: '/member-story-studio/list', // Redirect to the member story list // Updated
    meta: { breadcrumb: 'memberStory.list.title' }, // Updated
  },
  {
    path: '/member-story-studio/list', // Updated
    name: 'MemberStoryList', // Updated
    component: MemberStoryListView, // Updated
    meta: { breadcrumb: 'memberStory.list.title' }, // Updated
  },
];
