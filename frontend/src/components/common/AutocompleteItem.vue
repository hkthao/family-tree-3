<template>
  <v-list-item :title="itemTitle" :subtitle="itemSubtitle">
    <template v-slot:prepend>
      <v-avatar v-if="item.avatarUrl" :image="item.avatarUrl" size="small"></v-avatar>
      <v-icon v-else size="small">{{ itemIcon }}</v-icon>
    </template>
  </v-list-item>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { Member } from '@/types/family/member';
import type { Family } from '@/types/family/family';

type AutocompleteItem = Member | Family;

const props = defineProps<{
  item: AutocompleteItem;
}>();

const isMember = computed(() => 'fullName' in props.item);

const itemTitle = computed(() => {
  return isMember.value ? (props.item as Member).fullName : (props.item as Family).name;
});

const itemSubtitle = computed(() => {
  return isMember.value ? undefined : (props.item as Family).address;
});

const itemIcon = computed(() => {
  return isMember.value ? 'mdi-account-circle' : 'mdi-account-group';
});
</script>
