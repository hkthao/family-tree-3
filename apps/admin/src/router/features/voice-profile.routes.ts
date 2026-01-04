import type { RouteRecordRaw } from 'vue-router';
import VoiceProfileListView from '@/views/voice-profile/VoiceProfileListView.vue';

export const voiceProfileRoutes: Array<RouteRecordRaw> = [
  {
    path: '/members/:memberId/voice-profiles',
    name: 'VoiceProfileList',
    component: VoiceProfileListView,
    props: true,
    meta: {
      requiresAuth: true,
      roles: ['Admin', 'Manager', 'Member'], // Adjust roles as needed
      title: 'voiceProfile.title',
    },
  },
];
