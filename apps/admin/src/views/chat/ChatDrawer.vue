<template>
  <v-navigation-drawer :model-value="modelValue" @update:modelValue="$emit('update:modelValue', $event)"
    :location="location" :width="width" :class="drawerClass">
    <v-card flat class="fill-height d-flex flex-column">
      <v-card-title class="text-center" v-if="familyName">
        {{ t('chat.drawer.chatWithFamilyTitle', { familyName }) }}
      </v-card-title>
      <v-card-text class="pa-0">
        <template v-if="currentFamilyId">
          <ChatView :family-id="String(currentFamilyId)" @close-chat-drawer="emit('update:modelValue', false)"
            @open-relationship-detection="openRelationshipDetection"
            @add-generated-member="handleAddGeneratedMember"
            @add-generated-event="handleAddGeneratedEvent" />
        </template>
        <template v-else>
          <div class="d-flex flex-column justify-center align-center">
            <v-icon size="64" class="mb-4">mdi-account-group</v-icon>
            <div class="text-h6 mb-4">{{ t('chat.drawer.selectFamilyPrompt') }}</div>
            <FamilyAutocomplete v-model="selectedFamilyForChat" :label="t('chat.drawer.selectFamilyLabel')"
              variant="outlined" class="mb-4 w-100"
              :rules="[(v: string) => !!v || t('chat.drawer.validation.selectFamilyRequired')]" :clearable="true" />
            <v-btn color="primary" :disabled="!selectedFamilyForChat" @click="startChatWithSelectedFamily">
              {{ t('chat.drawer.startChatButton') }}
            </v-btn>
          </div>
        </template>
      </v-card-text>
    </v-card>
  </v-navigation-drawer>

  <v-navigation-drawer v-model="showRelationshipDetectionDrawer" location="right" temporary width="450">
    <v-container fluid>
      <RelationshipDetector 
      :narrowView="true"
      :initial-family-id="relationshipDetectionFamilyId" />
    </v-container>
  </v-navigation-drawer>

  <!-- Member Add Drawer -->
  <v-navigation-drawer v-model="showMemberAddDrawer" location="right" temporary width="450">
    <MemberAddView 
      :family-id="String(currentFamilyId)"
      :initial-member-data="memberAddInitialData"
      @close="showMemberAddDrawer = false"
      @saved="handleSavedMember"
    />
  </v-navigation-drawer>

  <!-- Event Add Drawer -->
  <v-navigation-drawer v-model="showEventAddDrawer" location="right" temporary width="450">
    <EventAddView 
      :family-id="String(currentFamilyId)"
      :initial-event-data="eventAddInitialData"
      @close="showEventAddDrawer = false"
      @saved="handleSavedEvent"
    />
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { type PropType, ref, computed } from 'vue';
import { useRouter } from 'vue-router';

import ChatView from '@/views/chat/ChatView.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useI18n } from 'vue-i18n';
import { useFamilyName } from '@/composables/family';
import RelationshipDetector from '@/components/relationship/RelationshipDetector.vue';
import MemberAddView from '@/views/member/MemberAddView.vue'; // New import
import EventAddView from '@/views/event/EventAddView.vue';   // New import
import type { MemberDto, EventDto } from '@/types'; // Import types

const emit = defineEmits(['update:modelValue']);
const { t } = useI18n();
const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true,
  },
  location: {
    type: String as PropType<'right' | 'bottom' | 'end' | 'left' | 'start' | 'top'>,
    default: 'right',
  },
  width: {
    type: [String, Number],
    default: 450,
  },
  drawerClass: {
    type: String,
    default: 'pa-2',
  },
  familyId: {
    type: String,
    required: false,
    default: null,
  },
});

const router = useRouter();
const selectedFamilyForChat = ref<string | null>(null);
const showRelationshipDetectionDrawer = ref(false);
const relationshipDetectionFamilyId = ref<string | undefined>(undefined);

// New reactive variables for member and event add drawers
const showMemberAddDrawer = ref(false);
const memberAddInitialData = ref<MemberDto | null>(null);
const showEventAddDrawer = ref(false);
const eventAddInitialData = ref<EventDto | null>(null);

const openRelationshipDetection = (familyId: string) => {
  relationshipDetectionFamilyId.value = familyId;
  showRelationshipDetectionDrawer.value = true;
};

const handleAddGeneratedMember = (memberData: MemberDto) => {
  memberAddInitialData.value = { ...memberData, familyId: currentFamilyId.value };
  showMemberAddDrawer.value = true;
};

const handleAddGeneratedEvent = (eventData: EventDto) => {
  eventAddInitialData.value = { ...eventData, familyId: currentFamilyId.value };
  showEventAddDrawer.value = true;
};

const handleSavedMember = () => {
  showMemberAddDrawer.value = false;
  // Potentially refresh chat messages here if needed, or emit an event to ChatView
};

const handleSavedEvent = () => {
  showEventAddDrawer.value = false;
  // Potentially refresh chat messages here if needed, or emit an event to ChatView
};

const currentFamilyId = computed(() => {
  if (props.familyId) {
    return props.familyId;
  }
  if (router.currentRoute.value.name === "FamilyDetail" && typeof router.currentRoute.value.params.id === 'string') {
    return router.currentRoute.value.params.id;
  }
  return "";
});

const { state: { familyName } } = useFamilyName(currentFamilyId);

const startChatWithSelectedFamily = () => {
  if (selectedFamilyForChat.value) {
    emit('update:modelValue', false);
  }
};

</script>

<style scoped>
/* Add any specific styles for the ChatDrawer here */
</style>