<template>
  <v-navigation-drawer :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)"
    :location="location" :width="width" :class="drawerClass">
    <v-card flat class="fill-height d-flex flex-column">
      <v-card-title class="text-center" v-if="familyName">
        {{ familyName }}
        <v-spacer></v-spacer>
      </v-card-title>
      <template v-if="currentFamilyId">
        <ChatView :family-id="String(currentFamilyId)" @close-chat-drawer="emit('update:modelValue', false)" />
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
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { type PropType, ref, computed } from 'vue';
import { useRouter } from 'vue-router';

import ChatView from '@/views/chat/ChatView.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useI18n } from 'vue-i18n';
import { useFamilyName } from '@/composables/family';

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