<template>
  <v-data-table-server
    v-model:items-per-page="itemsPerPage"
    :headers="headers"
    :items="events"
    :items-length="totalEvents"
    :loading="loading"
    item-value="id"
    @update:options="loadEvents"
    elevation="0"
  >
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('event.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('create')">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </v-toolbar>
    </template>

    <!-- Date column -->
    <template #item.startDate="{ item }">
      {{ formatDate(item.startDate) }}
    </template>

    <!-- Related Members column -->
    <template #item.relatedMembers="{ item }">
      <v-chip-group>
        <v-chip v-for="memberId in item.relatedMembers" :key="memberId" size="small">
          <v-avatar start>
            <v-img :src="getMemberAvatar(memberId)"></v-img>
          </v-avatar>
          {{ getMemberName(memberId) }}
        </v-chip>
      </v-chip-group>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-btn icon size="small" variant="text" @click="editEvent(item)">
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
      <v-btn icon size="small" variant="text" @click="confirmDelete(item)">
        <v-icon>mdi-delete</v-icon>
      </v-btn>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Event } from '@/types/event';
import type { DataTableHeader } from 'vuetify';
import { useMembers } from '@/data/members';
import type { Member } from '@/types/member';

defineProps({
  events: {
    type: Array as () => Event[],
    required: true,
  },
  totalEvents: {
    type: Number,
    required: true,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);

const { t } = useI18n();
const { getMembers } = useMembers();

const allMembers = ref<Member[]>([]);

const itemsPerPage = ref(10);

const headers = computed<DataTableHeader[]>(() => [
  { title: t('event.list.headers.date'), key: 'startDate', width: '120px', align: 'center' },
  { title: t('event.list.headers.name'), key: 'name', width: 'auto', align: 'start' },
  { title: t('event.list.headers.relatedMembers'), key: 'relatedMembers', width: '150px', align: 'start', sortable: false },
  { title: t('event.list.headers.location'), key: 'location', width: '150px', align: 'start' },
  { title: t('event.list.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);

import { formatDate } from '@/utils/dateUtils';

const getMemberName = (memberId: string) => {
  const member = allMembers.value.find(m => m.id === memberId);
  return member ? member.fullName : 'Unknown';
};

const getMemberAvatar = (memberId: string) => {
  const member = allMembers.value.find(m => m.id === memberId);
  return member ? member.avatarUrl : '';
};

const loadEvents = (options: { page: number; itemsPerPage: number; sortBy: string | string[] | null }) => {
  emit('update:options', options);
};

const editEvent = (event: Event) => {
  emit('edit', event);
};

const confirmDelete = (event: Event) => {
  emit('delete', event);
};

// Fetch all members on component mount
import { onMounted } from 'vue';
onMounted(async () => {
  const { members: fetchedMembers } = await getMembers({}, 1, -1); // Fetch all members
  allMembers.value = fetchedMembers;
});
</script>
