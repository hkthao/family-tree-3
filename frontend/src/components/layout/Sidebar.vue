<template>
  <v-navigation-drawer app>
    <v-list-item class="pa-4">
      <template v-slot:prepend>
        <v-icon color="primary">mdi-family-tree</v-icon>
      </template>
      <v-list-item-title class="text-h6 font-weight-bold">FamilyTree</v-list-item-title>
    </v-list-item>

    <v-divider></v-divider>

    <v-list density="comfortable" nav>
      <template v-for="(section, i) in filteredMenu" :key="i">
        <v-list-subheader>{{ section.title }}</v-list-subheader>
        <v-list-item
          v-for="(item, j) in section.items"
          :key="j"
          :to="item.to"
          :prepend-icon="item.icon"
          :title="$t(item.titleKey)"
          class="text-subtitle-1"
          active-class="active-item"
        ></v-list-item>
      </template>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import menu, { MenuSection } from '@/data/menuItems';
import { canAccessMenu } from '@/utils/menu-permissions';

const props = defineProps({
  currentUser: {
    type: Object,
    default: () => ({ id: 'u1', name: 'John', roles: ['FamilyManager'] })
  }
});

const { t } = useI18n();

const filteredMenu = computed(() => {
  const userRoles = props.currentUser.roles;
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
