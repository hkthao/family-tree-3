<template>
  <v-breadcrumbs class="breadcrumbs" :items="breadcrumbs" >
    <template v-slot:item="{ item }">
      <v-breadcrumbs-item v-bind="item">
        {{ item.title }}
      </v-breadcrumbs-item>
    </template>
    <template v-slot:divider>
      <v-icon icon="mdi-chevron-right"></v-icon>
    </template>
  </v-breadcrumbs>
</template>

<script setup lang="ts">
import { useAppBreadcrumbs } from '@/composables/ui/useAppBreadcrumbs';

const { breadcrumbs } = useAppBreadcrumbs();
</script>
<style scoped>
.breadcrumbs{
  padding-bottom: 0px;
}
</style>