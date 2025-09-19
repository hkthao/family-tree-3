<template>
  <v-navigation-drawer
    v-model="drawer"
    app
    class="border-right"
    :width="260"
  >
    <v-list-item class="pa-4">
      <template v-slot:prepend>
        <v-icon color="primary">mdi-family-tree</v-icon>
      </template>
      <v-list-item-title class="text-h6 font-weight-bold">FamilyTree</v-list-item-title>
    </v-list-item>

    <v-divider></v-divider>

    <v-list density="comfortable" nav>
      <v-list-subheader>APPS & PAGES</v-list-subheader>
      <v-list-item prepend-icon="mdi-view-dashboard-outline" title="Dashboards" value="dashboards"></v-list-item>
      <v-list-item prepend-icon="mdi-post-outline" title="Front Pages" value="front-pages"></v-list-item>
      <v-list-item prepend-icon="mdi-email-outline" title="Email" value="email"></v-list-item>
      <v-list-item prepend-icon="mdi-forum-outline" title="Chat" value="chat"></v-list-item>
      <v-list-item prepend-icon="mdi-calendar-blank-outline" title="Calendar" value="calendar"></v-list-item>
      <v-list-item prepend-icon="mdi-view-kanban-outline" title="Kanban" value="kanban">
        <template v-slot:append>
          <v-badge color="purple" content="Pro" inline rounded></v-badge>
        </template>
      </v-list-item>

      <v-list-subheader>USER INTERFACE</v-list-subheader>
      <v-list-item prepend-icon="mdi-account-cog-outline" title="Account Settings" value="account-settings"></v-list-item>
      <v-list-item prepend-icon="mdi-login" title="Login" value="login"></v-list-item>
      <v-list-item prepend-icon="mdi-account-plus-outline" title="Register" value="register"></v-list-item>

      <v-list-subheader>FORMS & TABLES</v-list-subheader>
      <v-list-item prepend-icon="mdi-file-document-outline" title="Form Layouts" value="form-layouts"></v-list-item>
      <v-list-item prepend-icon="mdi-table" title="Tables" value="tables"></v-list-item>
    </v-list>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const drawer = ref(true);
</script>
