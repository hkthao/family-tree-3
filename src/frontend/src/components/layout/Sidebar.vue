<template>
  <v-navigation-drawer app>
    <v-list-item class="pa-4">
      <AppNameDisplay />
    </v-list-item>

    <v-divider></v-divider>

    <v-list nav>
      <template v-for="(section, i) in filteredMenu" :key="i">
        <VListSubheader>{{ section.titleKey ? $t(section.titleKey) : section.title }}</VListSubheader>
        <v-list-item
          v-for="(item, j) in section.items"
          :key="j"
          :to="item.to"
          :prepend-icon="item.icon"
          :title="$t(item.titleKey)"
          active-class="active-item"
        ></v-list-item>
      </template>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { VListSubheader } from 'vuetify/components';
import menu from '@/data/menuItems';
import { canAccessMenu } from '@/utils/menuPermissions';
import type { User } from '@/types';
import { AppNameDisplay } from '@/components/common';

const props = defineProps({
  currentUser: {
    type: Object as () => User | null,
    required: false, // Not required as it can be null
  }
});

const filteredMenu = computed(() => {
  const userRoles = props.currentUser?.roles || [];
  return menu
    .map(section => ({
      ...section,
      items: section.items.filter(item => canAccessMenu(userRoles, item.roles))
    }))
    .filter(section => section.items.length > 0);
});
</script>

<style scoped>
.active-item {
  background-color: rgba(var(--v-theme-primary), 0.1);
  color: rgb(var(--v-theme-primary));
  border-radius: 8px;
}

.active-item .v-list-item-title {
  font-weight: bold;
}
</style>
