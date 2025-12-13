<template>
  <v-app-bar app flat>
    <v-app-bar-nav-icon @click.stop="$emit('toggle-drawer')"></v-app-bar-nav-icon>
    <!-- <v-text-field density="comfortable" variant="solo" prepend-inner-icon="mdi-magnify" :label="t('topbar.search')"
      single-line hide-details class="mx-4" @keydown.meta.k.prevent="focusSearch" ref="searchField" rounded flat>
    </v-text-field> -->
    <v-spacer></v-spacer>
    <v-btn icon @click="toggleTheme">
      <v-icon>mdi-theme-light-dark</v-icon>
    </v-btn>
    <NotificationBell />
    <!-- Chat Assistant Button -->
    <v-btn icon @click="showChatWidget = !showChatWidget">
      <v-icon>mdi-chat-processing</v-icon>
    </v-btn>
    <div class="mx-2">
      <UserMenu @navigate="handleNavigation" />
    </div>
  </v-app-bar>
  <!-- Chat Widget Component -->
  <N8nChatWidget v-model="showChatWidget"/>
</template>

<script setup lang="ts">
import { ref, type PropType, watch, onMounted } from 'vue'; // Added onMounted
import { useTheme } from 'vuetify';
import UserMenu from './UserMenu.vue';
import { useRouter } from 'vue-router';

import type { UserProfile } from '@/types';
import { useI18n } from 'vue-i18n';
import { useUserPreferenceStore } from '@/stores'; // Added useUserPreferenceStore
import { Theme } from '@/types';
import { getThemeOptions } from '@/constants/theme.constants';
import NotificationBell from '@/components/common/NotificationBell.vue';
import { N8nChatWidget } from '@/components/ai'; // Import ChatWidget

const { t } = useI18n();
const theme = useTheme();

const router = useRouter();
const userPreferenceStore = useUserPreferenceStore(); // Initialized useUserPreferenceStore

const showChatWidget = ref(false); // Reactive variable to control chat widget visibility

defineProps({
  currentUser: {
    type: Object as PropType<UserProfile | null>,
    required: false,
  },
});

defineEmits(['toggle-drawer']);

const getThemeCode = (theme: Theme) => {
  return getThemeOptions(t).find((option) => option.value === theme)!.code;
}

const toggleTheme = () => {
  const newTheme = theme.global.current.value.dark ? Theme.Light : Theme.Dark;
  userPreferenceStore.setTheme(newTheme); // Used userPreferenceStore
}

watch(() => userPreferenceStore.userPreference?.theme, (newTheme) => { // Used userPreferenceStore
  if (newTheme !== undefined) { // Check for undefined as userPreference might not be loaded yet
    theme.global.name.value = getThemeCode(newTheme);
  }
}, { immediate: true });

onMounted(async () => {
  await userPreferenceStore.fetchUserPreferences(); // Fetch preferences on mount
});

const handleNavigation = (route: string) => {
  router.push(route);
};
</script>