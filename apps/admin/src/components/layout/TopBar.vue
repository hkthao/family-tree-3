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
    <!-- Chat Button -->
    <v-btn icon @click="showChatDrawer = !showChatDrawer">
      <v-icon>mdi-chat-processing-outline</v-icon>
    </v-btn>

    <div class="mx-2">
      <UserMenu @navigate="handleNavigation" />
    </div>
  </v-app-bar>

  <ChatDrawer v-model="showChatDrawer" />
</template>


<script setup lang="ts">
import { ref, type PropType, watch } from 'vue';
import { useTheme } from 'vuetify';
import UserMenu from './UserMenu.vue';
import ChatDrawer from '@/views/chat/ChatDrawer.vue';
import { useRouter } from 'vue-router';

import type { UserProfile } from '@/types';
import { useI18n } from 'vue-i18n';
import { useUserPreferences } from '@/composables';
import { Theme } from '@/types';
import { getThemeOptions } from '@/constants/theme.constants';
import NotificationBell from '@/components/common/NotificationBell.vue';


const { t } = useI18n();
const theme = useTheme();
// const display = useDisplay(); // Initialize useDisplay

const router = useRouter();
const { state: { preferences }, actions: { savePreferences } } = useUserPreferences();

const showChatDrawer = ref(false);


// Computed property to determine if the chat drawer should be permanent
// const isPermanentChatDrawer = computed(() => display.mdAndUp.value);

// watch(isPermanentChatDrawer, (isPermanent) => {
//   if (isPermanent) {
//     showChatDrawer.value = true;
//   }
// }, { immediate: true });



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
  if (preferences.value) {
    savePreferences({ ...preferences.value, theme: newTheme });
  }
}

watch(() => preferences.value?.theme, (newTheme) => { // Used userPreferenceStore
  if (newTheme !== undefined) { // Check for undefined as userPreference might not be loaded yet
    theme.change(getThemeCode(newTheme));
  }
}, { immediate: true });

const handleNavigation = (route: string) => {
  router.push(route);
};
</script>