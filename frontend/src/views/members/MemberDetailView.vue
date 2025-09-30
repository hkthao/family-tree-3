<template>
    <v-card v-if="member" class="mb-4">
      <v-card-title class="text-h6 d-flex align-center">
        {{ member.fullName }}
        <v-spacer></v-spacer>
      </v-card-title>
      <v-card-text>
        <v-tabs v-model="selectedTab" class="mb-4">
          <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
          <v-tab value="timeline">{{ t('member.form.tab.timeline') }}</v-tab>
          <v-tab value="calendar">{{ t('event.view.calendar') }}</v-tab>
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
            />
          </v-window-item>

          <v-window-item value="calendar">
            <EventCalendar
              :member-id="member.id"
              :read-only="readOnly"
            />
          </v-window-item>
        </v-window>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="navigateToEditMember(member.id)">{{ t('common.edit') }}</v-btn>
        <v-btn color="blue-darken-1"  @click="closeView">{{ t('common.close') }}</v-btn>
      </v-card-actions>
    </v-card>
    <v-alert v-else-if="!loading" type="info" class="mt-4" variant="tonal">
      {{ t('common.noData') }}
    </v-alert>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useMemberStore } from '@/stores/member.store';
import MemberForm from '@/components/members/MemberForm.vue';
import EventTimeline from '@/components/events/EventTimeline.vue';
import EventCalendar from '@/components/events/EventCalendar.vue'; // Import EventCalendar
import type { Member } from '@/types/family';

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const memberStore = useMemberStore();

const member = ref<Member | undefined>(undefined);
const loading = ref(false);
const selectedTab = ref('general');
const readOnly = ref(true); // MemberDetailView is primarily for viewing

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

const closeView = () => {
  router.push('/members');
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
