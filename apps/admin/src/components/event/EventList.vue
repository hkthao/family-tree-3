<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="events"
    :items-length="totalEvents" :loading="loading" item-value="id" @update:options="loadEvents" elevation="0">
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title data-testid="event-list-title">{{ t('event.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>

        <v-btn color="primary" icon @click="$emit('create')" data-testid="add-new-event-button">
          <v-tooltip :text="t('event.list.action.create')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
          hide-details clearable class="mr-2" data-test-id="event-list-search-input"></v-text-field>
      </v-toolbar>
    </template>

    <!-- Date column -->
    <template #item.date="{ item }">
      <template v-if="item.calendarType === CalendarType.Solar && item.solarDate">
        {{ formatDate(item.solarDate) }}
      </template>
      <template v-else-if="item.calendarType === CalendarType.Lunar && item.lunarDate">
        {{ t('event.lunarDateDisplay', { day: item.lunarDate.day, month: item.lunarDate.month, isLeapMonth: item.lunarDate.isLeapMonth }) }}
      </template>
      <template v-else>
        -
      </template>
    </template>

    <!-- Event Name column -->
    <template #item.name="{ item }">
      <a @click="$emit('view', item.id)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer"
        data-testid="event-name-link">
        {{ item.name }}
      </a>
    </template>

    <!-- Family column -->
    <template #item.familyId="{ item }">
      <FamilyName :name="item.familyName" :avatar-url="item.familyAvatarUrl" />
    </template>

    <!-- Related Members column -->
    <template #item.relatedMembers="{ item }">
      <div class="d-flex flex-wrap">
        <MemberName v-for="member in item.relatedMembers" :key="member.id" :fullName="member.fullName"
          :avatarUrl="member.avatarUrl" :gender="member.gender" />
      </div>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-tooltip :text="t('event.list.action.edit')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="editEvent(item.id)"
            data-testid="edit-event-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('event.list.action.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="confirmDelete(item.id)"
            data-testid="delete-event-button">
            <v-icon>mdi-delete</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import type { Event } from '@/types';
import FamilyName from '@/components/common/FamilyName.vue';
import MemberName from '@/components/member/MemberName.vue'; // Import MemberName
import { useEventListComposable } from '@/composables/event/useEventListComposable';
import { CalendarType } from '@/types/enums'; // Import CalendarType enum

const props = defineProps<{
  events: Event[];
  totalEvents: number;
  loading: boolean;
  search: string;
}>();

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',

  'update:search',
]);

const {
  t,
  debouncedSearch,
  itemsPerPage,
  headers,
  loadEvents,
  editEvent,
  confirmDelete,
  formatDate,
} = useEventListComposable(props, emit);
</script>