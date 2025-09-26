<template>
  <v-container fluid>
    <v-card v-if="member" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ member.fullName }}
        <v-spacer></v-spacer>
        <v-btn v-if="!readOnly" color="primary" @click="navigateToEditMember(member.id)">
          {{ t('common.edit') }}
        </v-btn>
      </v-card-title>
      <v-card-text>
        <v-tabs v-model="selectedTab" class="mb-4">
          <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
          <v-tab value="timeline">{{ t('member.form.tab.timeline') }}</v-tab>
        </v-tabs>

        <v-window v-model="selectedTab">
          <v-window-item value="general">
            <MemberForm
              :initial-member-data="member"
              :read-only="true"
              :title="t('member.detail.title')"
            />
          </v-window-item>

          <v-window-item value="timeline">
            <EventTimeline
              :member-id="member.id"
              :read-only="readOnly"
              @addEvent="handleAddTimelineEvent"
              @editEvent="handleEditTimelineEvent"
              @deleteEvent="handleDeleteTimelineEvent"
            />
          </v-window-item>
        </v-window>
      </v-card-text>
    </v-card>
    <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
      {{ t('common.noData') }}
    </v-alert>

    <!-- Dialog for Event Form -->
    <v-dialog v-model="eventFormDialog" max-width="800px">
      <EventForm
        v-if="selectedEventForForm"
        :initial-event-data="selectedEventForForm"
        :read-only="readOnly"
        :title="isEditEventMode ? t('event.form.editTitle') : t('event.form.addTitle')"
        @close="handleCancelEventForm"
        @submit="handleSaveEventForm"
      />
    </v-dialog>

    <!-- Confirm Delete Dialog for Events -->
    <ConfirmDeleteDialog
      :model-value="deleteConfirmDialog"
      :title="t('confirmDelete.title')"
      :message="
        t('event.list.confirmDelete', { name: eventToDelete?.name || '' })
      "
      @confirm="handleDeleteConfirm"
      @cancel="handleDeleteCancel"
    />

    <!-- Global Snackbar -->
    <v-snackbar
      v-if="notificationStore.snackbar"
      v-model="notificationStore.snackbar.show"
      :color="notificationStore.snackbar.color"
      timeout="3000"
    >
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import { useEventStore } from '@/stores/event.store';
import { useNotificationStore } from '@/stores/notification.store';
import MemberForm from '@/components/members/MemberForm.vue';
import EventTimeline from '@/components/events/EventTimeline.vue';
import EventForm from '@/components/events/EventForm.vue';
import ConfirmDeleteDialog from '@/components/common/ConfirmDeleteDialog.vue';
import type { Member } from '@/types/family';
import type { Event } from '@/types/event/event';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const memberStore = useMemberStore();
const eventStore = useEventStore();
const notificationStore = useNotificationStore();

const member = ref<Member | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');
const readOnly = ref(true); // MemberDetailView is primarily for viewing

// Event Form Dialog State
const eventFormDialog = ref(false);
const selectedEventForForm = ref<Event | undefined>(undefined);
const isEditEventMode = ref(false);

// Event Delete Dialog State
const deleteConfirmDialog = ref(false);
const eventToDelete = ref<Event | undefined>(undefined);

const loadMember = async () => {
  loading.value = true;
  const memberId = route.params.id as string;
  if (memberId) {
    member.value = await memberStore.fetchItemById(memberId);
  }
  loading.value = false;
};

const navigateToEditMember = (id: string) => {
  router.push(`/members/edit/${id}`);
};

// Event Handlers for Timeline
const handleAddTimelineEvent = () => {
  if (!member.value?.id) return;
  selectedEventForForm.value = {
    id: '',
    name: '',
    description: '',
    startDate: null,
    endDate: null,
    location: '',
    familyId: member.value.familyId || null, // Pre-fill familyId
    relatedMembers: [member.value.id], // Pre-fill related member
    type: 'Other',
    color: 'blue',
  };
  isEditEventMode.value = false;
  eventFormDialog.value = true;
};

const handleEditTimelineEvent = (event: Event) => {
  selectedEventForForm.value = { ...event };
  isEditEventMode.value = true;
  eventFormDialog.value = true;
};

const handleDeleteTimelineEvent = (event: Event) => {
  eventToDelete.value = event;
  deleteConfirmDialog.value = true;
};

const handleDeleteConfirm = async () => {
  if (!eventToDelete.value?.id) return;
  try {
    await eventStore.deleteItem(eventToDelete.value.id);
    notificationStore.showSnackbar(t('event.messages.deleteSuccess'), 'success');
    // After deletion, reload member and timeline events
    await loadMember(); // This will trigger EventTimeline to reload via memberId prop change
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.deleteError'), 'error');
  } finally {
    deleteConfirmDialog.value = false;
    eventToDelete.value = undefined;
  }
};

const handleDeleteCancel = () => {
  deleteConfirmDialog.value = false;
  eventToDelete.value = undefined;
};

const handleSaveEventForm = async (eventData: Event) => {
  if (!member.value?.id) return;
  try {
    if (isEditEventMode.value) {
      await eventStore.updateItem(eventData);
      notificationStore.showSnackbar(t('event.messages.updateSuccess'), 'success');
    } else {
      await eventStore.addItem(eventData);
      notificationStore.showSnackbar(t('event.messages.addSuccess'), 'success');
    }
    // After saving, reload member and timeline events
    await loadMember(); // This will trigger EventTimeline to reload via memberId prop change
    eventFormDialog.value = false;
    selectedEventForForm.value = undefined;
  } catch (error) {
    notificationStore.showSnackbar(t('event.messages.saveError'), 'error');
  }
};

const handleCancelEventForm = () => {
  eventFormDialog.value = false;
  selectedEventForForm.value = undefined;
};

onMounted(() => {
  loadMember();
});

watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadMember();
    }
  },
);
</script>
