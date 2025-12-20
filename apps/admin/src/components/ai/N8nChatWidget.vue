<template>
  <v-navigation-drawer v-model="chatOpen" location="right" temporary width="400" class="n8n-chat-window">
    <v-card flat class="fill-height d-flex flex-column">
      <v-sheet class="n8n-chat-container pa-0 d-flex flex-column" height="100%">
        <div class="mt-8 mb-1 pa-1 pb-0">
          <FamilyAutocomplete
            v-model="selectedFamilyId"
            :label="t('n8nChat.familySelectionLabel')"
            clearable
            hide-details
          />
        </div>
          <div id="n8n-chat-target" ></div>
          <v-btn class="btn-close" variant="text" density="compact" icon @click="toggleChat">
            <v-icon>mdi-close</v-icon>
          </v-btn>
      </v-sheet>
    </v-card>
  </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { useN8nChat } from '@/composables';
import '@n8n/chat/style.css';

const { t } = useI18n();

const chatOpen = ref(false);
const selectedFamilyId = ref<string | undefined>(undefined);

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

watch(() => props.modelValue, (newVal) => {
  chatOpen.value = newVal;
});

// Use the new composable
const { actions: { toggleChat: toggleChatComposale } } = useN8nChat(selectedFamilyId, chatOpen);

const toggleChat = () => {
  toggleChatComposale();
  emit('update:modelValue', chatOpen.value);
};
</script>

<style scoped>
.n8n-chat-window {
  display: flex;
  flex-direction: column;
  border-radius: 8px;
  overflow: hidden;
  position: relative;

}

#n8n-chat-target {
  height: calc(100vh - 140px) !important;
}

.btn-close {
  position: absolute;
  z-index: 10;
  top: 5px;
  left: 5px;
  color: rgb(var(--v-theme-primary));
}
</style>