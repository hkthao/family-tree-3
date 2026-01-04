<template>
  <div>
    <div v-if="isLoading" class="text-center py-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p>{{ t('common.loading') }}</p>
    </div>
    <v-alert v-else-if="error" type="error" class="mb-4">{{ error?.message || t('event.errors.load') }}</v-alert>
    <v-list v-else-if="memberEvents && memberEvents.length > 0">
      {{ console.log('MemberEventsTab - memberEvents:', memberEvents) }}
      <v-list-item v-for="event in memberEvents" :key="event.id" class="mb-2" :title="event.name"
        :subtitle="getEventSubtitle(event)">
        <template v-slot:prepend>
          <v-avatar color="primary" class="text-white">
            <v-icon>{{ getEventTypeIcon(event.type) }}</v-icon>
          </v-avatar>
        </template>
        <template v-slot:append>
          <v-btn icon variant="text" size="small" @click="showEventDetails(event.id)" v-if="event.id">
            <v-icon>mdi-information</v-icon>
            <v-tooltip activator="parent" location="top">{{ t('event.detail.title') }}</v-tooltip>
          </v-btn>
        </template>
      </v-list-item>
    </v-list>
    <v-alert v-else type="info" variant="tonal" class="ma-2">{{ t('event.list.noEvents') }}</v-alert>

    <BaseCrudDrawer v-model="detailDrawer" @close="handleDetailClosed">
      <EventDetailView v-if="detailDrawer && selectedEventId" :event-id="selectedEventId" @close="handleDetailClosed" />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberEventsQuery } from '@/composables/event/useMemberEventsQuery';
import type { EventDto } from '@/types';
import { format } from 'date-fns';
import { CalendarType, RepeatRule } from '@/types/enums';
import { getEventTypeIcon } from '@/composables/utils/eventOptions';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import EventDetailView from '@/views/event/EventDetailView.vue';

interface MemberEventsTabProps {
  memberId: string;
}

const props = defineProps<MemberEventsTabProps>();
const { t } = useI18n();

const { data: memberEvents, isLoading, error } = useMemberEventsQuery(props.memberId);

const detailDrawer = ref(false);
const selectedEventId = ref<string | null>(null);

const showEventDetails = (eventId: string) => {
  selectedEventId.value = eventId;
  detailDrawer.value = true;
};

const handleDetailClosed = () => {
  detailDrawer.value = false;
  selectedEventId.value = null;
};



const getEventSubtitle = (event: EventDto): string => {
  let subtitle = '';
  if (event.calendarType === CalendarType.Solar && event.solarDate) {
    subtitle += format(new Date(event.solarDate), 'dd/MM/yyyy');
  } else if (event.calendarType === CalendarType.Lunar && event.lunarDate) {
    subtitle += `${event.lunarDate.day}/${event.lunarDate.month} (Âm lịch)`;
  } else {
    subtitle += t('common.unknown');
  }

  if (event.repeatRule === RepeatRule.Yearly) {
    subtitle += ` (${t('event.repeatRule.yearly')})`;
  }

  if (event.location) {
    subtitle += ` - ${event.location}`;
  }

  return subtitle;
};
</script>

<style scoped></style>