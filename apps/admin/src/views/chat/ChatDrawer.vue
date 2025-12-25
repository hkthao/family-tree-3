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
            @open-relationship-detection="openRelationshipDetection" />
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

  <v-navigation-drawer v-model="showRelationshipDetectionDrawer" location="right" temporary :width="drawerWidth">
    <v-container fluid>
      <RelationshipDetector :narrowView="true" :initial-family-id="relationshipDetectionFamilyId" />
    </v-container>
  </v-navigation-drawer>

  <!-- Member Add Drawer -->
  <v-navigation-drawer v-model="showMemberAddDrawer" location="right" temporary :width="drawerWidth">
    <MemberAddView :family-id="String(currentFamilyId)" :initial-member-data="memberAddInitialData"
      @close="handleMemberFormCloseOrSaved" @saved="handleMemberFormCloseOrSaved" />
  </v-navigation-drawer>

  <!-- Event Add Drawer -->
  <v-navigation-drawer v-model="showEventAddDrawer" location="right" temporary :width="drawerWidth">
    <EventAddView :family-id="String(currentFamilyId)" :initial-event-data="eventAddInitialData"
      @close="handleEventFormCloseOrSaved" @saved="handleEventFormCloseOrSaved" :key="eventAddInitialData?.id" />
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { type PropType, ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { v4 as uuidv4 } from 'uuid';
import ChatView from '@/views/chat/ChatView.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useI18n } from 'vue-i18n';
import { useFamilyName } from '@/composables/family';
import RelationshipDetector from '@/components/relationship/RelationshipDetector.vue';
import MemberAddView from '@/views/member/MemberAddView.vue';
import EventAddView from '@/views/event/EventAddView.vue';
import { useGeneratedDataStore } from '@/stores/generatedData.store'; // New import

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

const drawerWidth = 550
const router = useRouter();
const selectedFamilyForChat = ref<string | null>(null);
const showRelationshipDetectionDrawer = ref(false);
const relationshipDetectionFamilyId = ref<string | undefined>(undefined);

// Use the generated data store
const generatedDataStore = useGeneratedDataStore();

// Reactive variables for member and event add drawers
const showMemberAddDrawer = ref(false);
const memberAddInitialData = ref<any | null>(null); // Use any for now to avoid type issues with MemberDto
const showEventAddDrawer = ref(false);
const eventAddInitialData = ref<any | null>(null); // Use any for now to avoid type issues with EventDto

watch(() => showMemberAddDrawer.value, (newVal) => {
  if (!newVal) {
    memberAddInitialData.value = null;
    generatedDataStore.clearMemberToAdd();
  }
});

watch(() => showEventAddDrawer.value, (newVal) => {
  if (!newVal) {
    eventAddInitialData.value = null;
    generatedDataStore.clearEventToAdd();
  }
});

watch(() => generatedDataStore.memberToAdd, (newVal) => {
  if (newVal) {
    memberAddInitialData.value = { ...newVal, familyId: currentFamilyId.value, id: uuidv4() };
    showMemberAddDrawer.value = true;
  } else {
    showMemberAddDrawer.value = false;
  }
});

watch(() => generatedDataStore.eventToAdd, (newVal) => {
  if (newVal) {
    eventAddInitialData.value = { ...newVal, familyId: currentFamilyId.value, id: uuidv4() };
    showEventAddDrawer.value = true;
  } else {
    showEventAddDrawer.value = false;
  }
});

const openRelationshipDetection = (familyId: string) => {
  relationshipDetectionFamilyId.value = familyId;
  showRelationshipDetectionDrawer.value = true;
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

const handleMemberFormCloseOrSaved = () => {
  generatedDataStore.clearMemberToAdd();
};

const handleEventFormCloseOrSaved = () => {
  generatedDataStore.clearEventToAdd();
};

</script>