<template>
  <v-data-table-server :headers="state.headers.value" :items="events"
    :items-length="totalEvents" :loading="loading" item-value="id" @update:options="actions.loadEvents" elevation="0"
    :page="props.page" :items-per-page="props.itemsPerPage">
    <template #top>
      <!-- REFACTOR: Use ListToolbar component -->
      <ListToolbar
        :title="t('event.list.title')"
        :search-query="state.debouncedSearch.value"
        :search-label="t('common.search')"
        :create-button-tooltip="t('event.list.action.create')"
        create-button-test-id="add-new-event-button"
        @update:search="emit('update:search', $event)"
        @create="emit('create')"
      >
        <template #custom-buttons>
          <!-- Export Button -->
          <v-btn
            v-if="props.canPerformActions"
            color="primary"
            icon
            @click="props.onExport?.()"
            data-testid="export-event-button"
            :aria-label="t('common.export')"
            :loading="props.isExporting"
          >
            <v-tooltip :text="t('common.export')">
              <template v-slot:activator="{ props: tooltipProps }">
                <v-icon v-bind="tooltipProps">mdi-export</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <!-- Import Button -->
          <v-btn
            v-if="props.canPerformActions"
            color="primary"
            icon
            @click="props.onImportClick?.()"
            data-testid="import-event-button"
            :aria-label="t('common.import')"
            :loading="props.isImporting"
          >
            <v-tooltip :text="t('common.import')">
              <template v-slot:activator="{ props: tooltipProps }">
                <v-icon v-bind="tooltipProps">mdi-import</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <!-- Generate Event Occurrences Button (Admin Only) -->
          <v-btn
            v-if="props.isAdmin"
            color="primary"
            icon
            @click="emit('generateOccurrences', new Date().getFullYear(), props.familyId)"
            data-testid="generate-event-occurrences-button"
            :aria-label="t('event.list.action.generateOccurrences')"
            :loading="props.isGeneratingOccurrences"
          >
            <v-tooltip :text="t('event.list.action.generateOccurrences')">
              <template v-slot:activator="{ props: tooltipProps }">
                <v-icon v-bind="tooltipProps">mdi-calendar-plus</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
          <!-- Generate and Notify Events Button (Admin Only) -->
          <v-btn
            v-if="props.isAdmin"
            color="primary"
            icon
            @click="emit('generateAndNotify', props.familyId)"
            data-testid="generate-and-notify-events-button"
            :aria-label="t('event.list.action.generateAndNotify')"
            :loading="props.isGeneratingAndNotifying"
          >
            <v-tooltip :text="t('event.list.action.generateAndNotify')">
              <template v-slot:activator="{ props: tooltipProps }">
                <v-icon v-bind="tooltipProps">mdi-bell-alert</v-icon>
              </template>
            </v-tooltip>
          </v-btn>
        </template>
      </ListToolbar>
    </template>

    <!-- Date column -->
    <template #item.date="{ item }">
      <template v-if="item.calendarType === CalendarType.Solar && item.solarDate">
        {{ formatDate(item.solarDate) }}
      </template>
      <template v-else-if="item.calendarType === CalendarType.Lunar && item.lunarDate">
        {{ t('event.lunarDateDisplay', {
          day: item.lunarDate.day, month: item.lunarDate.month, isLeapMonth:
            item.lunarDate.isLeapMonth
        }) }}
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

    <!-- Current Year Occurrence Date column -->
    <template #item.currentYearOccurrenceDate="{ item }">
      {{ item.currentYearOccurrenceDate ? formatDate(item.currentYearOccurrenceDate) : '-' }}
    </template>

    <!-- Event Members column -->
    <template #item.eventMembers="{ item }">
      <div class="d-flex flex-wrap">
        <MemberName v-for="member in item.eventMembers" :key="member.memberId" :fullName="member.fullName"
          :avatarUrl="member.avatarUrl" :gender="member.gender" />
      </div>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-tooltip :text="t('event.list.action.edit')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="actions.editEvent(item.id)"
            data-testid="edit-event-button">
            <v-icon>mdi-pencil</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('event.list.action.sendNotification')">
        <template v-slot:activator="{ props }">
          <v-btn
            v-if="isAdmin"
            icon
            size="small"
            variant="text"
            v-bind="props"
            @click="emit('sendNotification', item.id)"
            data-testid="send-notification-button"
          >
            <v-icon>mdi-send</v-icon>
          </v-btn>
        </template>
      </v-tooltip>
      <v-tooltip :text="t('event.list.action.delete')">
        <template v-slot:activator="{ props }">
          <v-btn icon size="small" variant="text" v-bind="props" @click="actions.confirmDelete(item.id)"
            data-testid="delete-event-button"> <v-icon>mdi-delete</v-icon>
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
import type { EventDto } from '@/types';
import FamilyName from '@/components/common/FamilyName.vue';
import MemberName from '@/components/member/MemberName.vue';
import { useEventListComposable } from '@/composables';
import { CalendarType } from '@/types/enums';
import ListToolbar from '@/components/common/ListToolbar.vue'; // NEW
import { formatDate } from '@/utils/dateUtils'; // NEW

import { useI18n } from 'vue-i18n'; // NEW

const { t } = useI18n(); // Destructure t from useI18n

const props = defineProps<{
  canPerformActions?: boolean;
  events: EventDto[];
  familyId?: string;
  isExporting?: boolean;
  isGeneratingAndNotifying?: boolean; // NEW
  isGeneratingOccurrences?: boolean;
  isImporting?: boolean;
  isAdmin?: boolean;
  isSendingNotification?: boolean;
  itemsPerPage?: number;
  loading: boolean;
  onExport?: () => void;
  onGenerateAndNotify?: (familyId?: string) => void; // NEW
  onImportClick?: () => void;
  page?: number;
  search: string;
  totalEvents: number;
}>();

const emit = defineEmits([
  'create',
  'delete',
  'edit',
  'generateAndNotify', // NEW
  'generateOccurrences',
  'sendNotification',
  'update:options',
  'update:search',
  'view',
]);

const { state, actions } = useEventListComposable(props, emit);
</script>

<style scoped></style>
