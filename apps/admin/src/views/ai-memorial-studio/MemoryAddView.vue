<template>
  <v-form ref="form" @submit.prevent="handleSubmit">
    <v-card-title>
      <span class="text-h6">{{ t('memory.create.title') }}</span>
    </v-card-title>
    <v-card-text>
      <v-container>
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="editedMemory.title"
              :label="t('memory.storyEditor.title')"
              :rules="[(v: string) => !!v || t('common.validations.required')]"
              required
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-textarea
              v-model="editedMemory.story"
              :label="t('memory.storyEditor.storyContent')"
              :rules="[(v: string) => !!v || t('common.validations.required')]"
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
          <v-col cols="12">
            <MemberAutocomplete
              v-model="editedMemory.memberId"
              :label="t('member.form.member')"
              :rules="[(v: string) => !!v || t('common.validations.required')]"
              :read-only="!!props.memberId"
              required
            ></MemberAutocomplete>
          </v-col>
        </v-row>
      </v-container>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="handleClose">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn color="blue-darken-1" variant="text" type="submit" :loading="memoryStore.add.loading">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemoryStore } from '@/stores/memory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { CreateMemoryDto } from '@/types/memory';
import { MemberAutocomplete } from '@/components/common';

const props = defineProps<{
  memberId?: string; // Optional memberId for pre-filling
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memoryStore = useMemoryStore();
const { showSnackbar } = useGlobalSnackbar();

const form = ref<HTMLFormElement | null>(null);

const editedMemory = ref<CreateMemoryDto>({
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  story: '',
  photoAnalysisId: undefined,
  photoUrl: undefined,
  tags: [],
  keywords: [],
});

const handleSubmit = async () => {
  if (form.value && (await form.value.validate()).valid) {
    const result = await memoryStore.addItem(editedMemory.value);
    if (result.ok) {
      showSnackbar(t('memory.create.step5.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(t('memory.create.step5.saveFailed'), 'error');
    }
  }
};

const handleClose = () => {
  emit('close');
};
</script>