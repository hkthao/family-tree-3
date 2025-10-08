<template>
  <v-menu
    offset-y
    :placement="placement"
    :close-on-content-click="false"
    v-model="menuOpen"
  >
    <template v-slot:activator="{ props: activatorProps }">
              <AvatarDisplay
                :src="currentUser?.avatar"
                :size="36"
                class="cursor-pointer"
                :aria-label="$t('userMenu.ariaLabel')"
                aria-haspopup="true"
                :aria-expanded="menuOpen"
                v-bind="activatorProps"
              />
    </template>

    <v-sheet class="user-menu-sheet rounded-lg elevation-3 pa-3">
      <!-- Header -->
      <div class="user-menu-header d-flex flex-column align-center">
        <AvatarDisplay :src="currentUser?.avatar" :size="56" />

        <div class="d-flex flex-column align-center mt-2">
          <span class="text-h6 font-weight-medium">{{ currentUser?.name }}</span>
          <v-chip label size="small" color="primary" variant="tonal" class="mt-1">
            {{ $t('userMenu.role') }}: {{ currentUser?.roles?.[0] }}
          </v-chip>
        </div>
      </div>

      <!-- Menu Items -->
      <v-list dense nav>
        <v-list-item
          v-for="item in menuItems"
          :key="item.key"
          :to="item.to"
          :prepend-icon="item.icon"
          @click="handleMenuItemClick(item.to)"
          class="user-menu-item"
        >
          <v-list-item-title>{{ $t(item.labelKey) }}</v-list-item-title>
          <v-tooltip activator="parent" location="right">{{ $t(item.labelKey) }}</v-tooltip>
        </v-list-item>
      </v-list>

      <v-divider class="my-2"></v-divider>

      <!-- Logout -->
      <v-list-item
        prepend-icon="mdi-logout"
        color="error"
        @click="confirmLogoutDialog = true"
        class="user-menu-item"
      >
        <v-list-item-title>{{ $t('userMenu.logout') }}</v-list-item-title>
        <v-tooltip activator="parent" location="right">{{ $t('userMenu.logout') }}</v-tooltip>
      </v-list-item>
    </v-sheet>
  </v-menu>

  <!-- Confirm Logout Dialog -->
  <v-dialog v-model="confirmLogoutDialog" max-width="350">
    <v-card>
      <v-card-title class="headline">{{ $t('userMenu.logout') }}</v-card-title>
      <v-card-text>{{ $t('userMenu.confirmLogout') }}</v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" text @click="confirmLogoutDialog = false">{{ $t('userMenu.cancel') }}</v-btn>
        <v-btn color="error" text @click="handleLogoutConfirm">{{ $t('userMenu.logout') }}</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { userMenuItems } from '@/data/userMenuItems';
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth.store';
import { AvatarDisplay } from '@/components/common';

const menuItems = userMenuItems;

defineProps({
  placement: {
    type: String as () => 'bottom' | 'bottom-end' | 'bottom-start',
    default: 'bottom-end',
  },
});

const emit = defineEmits(['navigate']);

const menuOpen = ref(false);
const confirmLogoutDialog = ref(false);

const authStore = useAuthStore();
const currentUser = computed(() => authStore.user);
const router = useRouter();

const handleMenuItemClick = (route?: string) => {
  if (route) {
    emit('navigate', route);
  }
  menuOpen.value = false;
};

const handleLogoutConfirm = async () => {
  confirmLogoutDialog.value = false;
  await authStore.logout();
  router.push({ name: 'Login' }); // Redirect to login page
};</script>

<style scoped>

.user-menu-header {
  display: flex;
  align-items: center;
  justify-content: center; /* Added for centering */
  gap: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid rgba(var(--v-border-color), 0.12);
  margin-bottom: 8px;
}
</style>
