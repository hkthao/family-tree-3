<template>
  <v-form ref="form" @submit.prevent="handleSubmit">
    <v-card-title>
      <span class="text-h6">{{ t('memory.edit.title') }}</span>
    </v-card-title>
    <v-card-text>
      <v-container>
        <v-alert v-if="memoryStore.error" type="error" dismissible class="mb-4">
          {{ memoryStore.error }}
        </v-alert>
        <v-row v-if="editedMemory">
          <v-col cols="12">
            <v-text-field
              v-model="editedMemory.title"
              :label="t('memory.storyEditor.title')"
              :rules="[v => !!v || t('common.validations.required')]"
              required
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-textarea
              v-model="editedMemory.story"
              :label="t('memory.storyEditor.storyContent')"
              :rules="[v => !!v || t('common.validations.required')]"
              required
            ></v-textarea>
          </v-col>
          <v-col cols="12">
            <v-text-field
              v-model="editedMemory.photoUrl"
              :label="t('memory.create.step1.choosePhoto')"
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-combobox
              v-model="editedMemory.tags"
              :label="t('memory.storyEditor.tags')"
              chips
              multiple
              clearable
            ></v-combobox>
          </v-col>
          <v-col cols="12">
            <v-combobox
              v-model="editedMemory.keywords"
              :label="t('memory.storyEditor.keywords')"
              chips
              multiple
              clearable
            ></v-combobox>
          </v-col>
        </v-row>
        <v-row v-else>
          <v-col cols="12">
            <v-alert type="info">{{ t('memory.edit.loading') }}</v-alert>
          </v-col>
        </v-row>
      </v-container>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="handleClose">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn color="blue-darken-1" variant="text" type="submit" :loading="memoryStore.update.loading">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-form>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemoryDto, UpdateMemoryDto } from '@/types/memory';

const props = defineProps<{
  memoryId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const form = ref<HTMLFormElement | null>(null);
const editedMemory = ref<UpdateMemoryDto | null>(null);

const fetchMemory = async (id: string) => {
  const memory = await memoryStore.getById(id);
  if (memory) {
    editedMemory.value = {
      id: memory.id,
      memberId: memory.memberId,
      title: memory.title,
      story: memory.story,
      photoAnalysisId: memory.photoAnalysisId,
      photoUrl: memory.photoUrl,
      tags: memory.tags,
      keywords: memory.keywords,
    };
  } else {
    showSnackbar(t('memory.edit.notFound'), 'error');
    emit('close');
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

const handleSubmit = async () => {
  if (form.value && (await form.value.validate()).valid && editedMemory.value) {
    const result = await memoryStore.updateItem(editedMemory.value);
    if (result.ok) {
      showSnackbar(t('memory.edit.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(t('memory.edit.saveFailed'), 'error'); // Need to add this translation
    }
  }
};

const handleClose = () => {
  emit('close');
};
</script>