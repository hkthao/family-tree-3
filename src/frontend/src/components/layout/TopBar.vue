<template>
  <v-app-bar app flat>
    <v-app-bar-nav-icon @click.stop="$emit('toggle-drawer')"></v-app-bar-nav-icon>
    <v-text-field density="comfortable" variant="solo" prepend-inner-icon="mdi-magnify" :label="t('topbar.search')"
      single-line hide-details class="mx-4" @keydown.meta.k.prevent="focusSearch" ref="searchField" rounded flat>
    </v-text-field>
    <v-spacer></v-spacer>
    <v-btn icon @click="toggleTheme">
      <v-icon>mdi-theme-light-dark</v-icon>
    </v-btn>
    <NotificationBell />
    <div class="mx-2">
      <UserMenu @navigate="handleNavigation" />
    </div>
  </v-app-bar>
</template>

<script setup lang="ts">
import { ref, type PropType, watch } from 'vue';
import { useTheme } from 'vuetify';
import UserMenu from './UserMenu.vue';
import { useRouter } from 'vue-router';
import type { VTextField } from 'vuetify/components';
import type { User } from '@/types';
import { useI18n } from 'vue-i18n';
import { useUserSettingsStore } from '@/stores';
import { Theme } from '@/types';
import { getThemeOptions } from '@/constants/theme.constants';
import NotificationBell from '@/components/common/NotificationBell.vue';

const { t } = useI18n();
const theme = useTheme();
const searchField = ref<VTextField | null>(null);
const router = useRouter();
const userSettingsStore = useUserSettingsStore();

defineProps({
  currentUser: {
    type: Object as PropType<User | null>,
    required: false,
  },
});

defineEmits(['toggle-drawer']);

const getThemeCode = (theme: Theme) => {
  return getThemeOptions(t).find((option) => option.value === theme)!.code;
}

const toggleTheme = () => {
  const newTheme = theme.global.current.value.dark ? Theme.Light : Theme.Dark;
  userSettingsStore.setTheme(newTheme);
}

watch(() => userSettingsStore.preferences.theme, (newTheme) => {
  theme.change(getThemeCode(newTheme))
}, { immediate: true });

const focusSearch = () => {
  if (searchField.value) {
    searchField.value.focus();
  }
}

const handleNavigation = (route: string) => {
  router.push(route);
};
</script>
