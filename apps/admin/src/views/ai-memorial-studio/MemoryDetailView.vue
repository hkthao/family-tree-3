<template>
  <v-card-title>
    <span class="text-h6">{{ memory?.title || t('memory.detail.titleDefault') }}</span>
  </v-card-title>
  <v-card-text>
    <v-alert v-if="memoryStore.error" type="error" dismissible class="mb-4">
      {{ memoryStore.error }}
    </v-alert>
    <v-row v-if="memory">
      <v-col cols="12">
        <v-list density="compact">
          <v-list-item>
            <v-list-item-title>{{ t('memory.storyEditor.storyContent') }}:</v-list-item-title>
            <v-list-item-subtitle>{{ memory.story }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memory.memberId">
            <v-list-item-title>{{ t('member.form.memberId') }}:</v-list-item-title>
            <v-list-item-subtitle>{{ memory.memberId }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memory.photoUrl">
            <v-list-item-title>{{ t('memory.create.step1.choosePhoto') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <a :href="memory.photoUrl" target="_blank">{{ memory.photoUrl }}</a>
            </v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memory.tags && memory.tags.length > 0">
            <v-list-item-title>{{ t('memory.storyEditor.tags') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <v-chip v-for="tag in memory.tags" :key="tag" size="small" class="mr-1 my-1">{{ tag }}</v-chip>
            </v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memory.keywords && memory.keywords.length > 0">
            <v-list-item-title>{{ t('memory.storyEditor.keywords') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <v-chip v-for="keyword in memory.keywords" :key="keyword" size="small" class="mr-1 my-1">{{ keyword }}</v-chip>
            </v-list-item-subtitle>
          </v-list-item>
          <v-list-item>
            <v-list-item-title>{{ t('memory.list.header.createdAt') }}:</v-list-item-title>
            <v-list-item-subtitle>{{ formatDate(memory.createdAt) }}</v-list-item-subtitle>
          </v-list-item>
          <v-list-item v-if="memory.photoAnalysisResult">
            <v-list-item-title>{{ t('memory.detail.photoAnalysisResult') }}:</v-list-item-title>
            <v-list-item-subtitle>
              <!-- Display a summary or link to full analysis -->
              {{ memory.photoAnalysisResult.description || t('common.noData') }}
            </v-list-item-subtitle>
          </v-list-item>
        </v-list>
      </v-col>
    </v-row>
    <v-row v-else>
      <v-col cols="12">
        <v-alert type="info">{{ t('memory.detail.loading') }}</v-alert>
      </v-col>
    </v-row>
  </v-card-text>
  <v-card-actions>
    <v-spacer></v-spacer>
    <v-btn color="blue-darken-1" variant="text" @click="emit('close')">
      {{ t('common.close') }}
    </v-btn>
    <v-btn color="blue-darken-1" variant="text" @click="emit('edit-item', memory?.id)">
      {{ t('common.edit') }}
    </v-btn>
  </v-card-actions>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemoryDto } from '@/types/memory'; // Make sure this import is correct
import i18n from '@/plugins/i18n'; // Import i18n instance for formatDate

const props = defineProps<{
  memoryId: string;
}>();

const emit = defineEmits(['close', 'edit-item']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memory = ref<MemoryDto | null>(null);

const fetchMemory = async (id: string) => {
  memory.value = await memoryStore.getById(id) || null;
  if (!memory.value) {
    showSnackbar(t('memory.detail.notFound'), 'error');
  }
};

onMounted(() => {
  if (props.memoryId) {
    fetchMemory(props.memoryId);
  }
});

watch(() => props.memoryId, (newId) => {
  if (newId) {
    fetchMemory(newId);
  }
});

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString(i18n.global.locale.value);
};
</script>