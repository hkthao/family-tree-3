<template>
  <v-card flat>
    <v-card-title class="d-flex align-center">
      <v-btn icon @click="emit('close')" variant="text">
        <v-icon>mdi-close</v-icon>
      </v-btn>
      <span class="text-h6">{{ t('memory.edit.title') }}</span>
      <v-spacer></v-spacer>
    </v-card-title>
    <v-card-text>
      <v-container v-if="loading">
        <v-row>
          <v-col cols="12" class="text-center">
            <v-progress-circular indeterminate color="primary"></v-progress-circular>
            <p class="mt-2">{{ t('memory.edit.loading') }}</p>
          </v-col>
        </v-row>
      </v-container>
      <v-container v-else-if="editableMemory">
        <v-row>
          <v-col cols="12">
            <StoryEditor :draft="editableMemory" @update:draft="onUpdateEditableMemory" />
          </v-col>
          <v-col cols="12">
            <v-img v-if="editableMemory.photoUrl" :src="editableMemory.photoUrl" max-height="200" contain class="mb-4"></v-img>
          </v-col>
        </v-row>
      </v-container>
      <v-container v-else>
        <v-alert type="error">{{ t('memory.edit.notFound') }}</v-alert>
      </v-container>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" @click="saveMemory" :disabled="!editableMemory || savingMemory" :loading="savingMemory">
        {{ t('common.save') }}
      </v-btn>
      <v-btn color="secondary" @click="emit('close')" :disabled="savingMemory">
        {{ t('common.cancel') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import StoryEditor from './StoryEditor.vue';

interface Props {
  memoryId: string;
}
const props = defineProps<Props>();
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const editableMemory = ref<any | null>(null);
const loading = ref(false);
const savingMemory = ref(false);

  const loadMemory = async (id: string) => {
  loading.value = true;
  const memoryData = await memoryStore.getById(id);
  if (memoryData) {
    editableMemory.value = { ...memoryData }; 
  } else {
    editableMemory.value = null;
  }
  loading.value = false;
};

const onUpdateEditableMemory = (draft: any) => {
  editableMemory.value = { ...draft };
};

const saveMemory = async () => {
  if (!editableMemory.value) return;

  savingMemory.value = true;
  const updatePayload = {
    id: props.memoryId,
    memberId: editableMemory.value.memberId,
    title: editableMemory.value.title,
    story: editableMemory.value.draftStory || editableMemory.value.story,
    photoAnalysisId: editableMemory.value.photoAnalysisId,
    photoUrl: editableMemory.value.photoUrl,
    tags: editableMemory.value.tags,
    keywords: editableMemory.value.keywords,
  };

  const result = await memoryStore.updateItem(updatePayload);
  if (result.ok) {
    emit('saved', props.memoryId);
  } else {
    // Error is handled by the store, no specific action needed here
  }
  savingMemory.value = false;
};
onMounted(() => {
  if (props.memoryId) {
    loadMemory(props.memoryId);
  }
});

watch(
  () => props.memoryId,
  (newId) => {
    if (newId) {
      loadMemory(newId);
    } else {
      editableMemory.value = null;
    }
  },
);
</script>

<style scoped>
/* Scoped styles for MemoryEdit */
</style>
