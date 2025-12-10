<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.edit.title') }}</span>
    </v-card-title>
    <v-card-text v-if="loading">
      <v-progress-linear indeterminate color="primary"></v-progress-linear>
      <div class="text-center mt-2">{{ t('memberStory.edit.loading') }}</div>
    </v-card-text>
    <v-card-text v-else-if="!editedMemberStory">
      <v-alert type="error">{{ t('memberStory.edit.notFound') }}</v-alert>
    </v-card-text>
    <MemberStoryForm v-else
      ref="memberStoryFormRef"
      v-model="editedMemberStory"
    />
    <v-card-actions v-if="editedMemberStory">
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="handleClose">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving" :disabled="!memberStoryFormRef?.isValid">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory';
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue'; // Updated import

const props = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memberStoryFormRef = ref<InstanceType<typeof MemberStoryForm> | null>(null); // Updated type
const editedMemberStory = ref<MemberStoryDto | null>(null);
const loading = ref(false);
const isSaving = ref(false);

const loadMemberStory = async (id: string) => {
  loading.value = true;
  try {
    const result: MemberStoryDto | undefined = await memberStoryStore.getById(id);
    if (result) {
      editedMemberStory.value = {
        ...result,
        // rawInput no longer exists on MemberStoryDto, so no need to map
      };
    } else {
      editedMemberStory.value = null; // Ensure it's null if not found
      showSnackbar(t('memberStory.edit.notFound'), 'error');
    }
  } catch (error) {
    console.error('Error loading member story:', error);
    showSnackbar((error as Error).message, 'error');
  } finally {
    loading.value = false;
  }
};

const handleSave = async () => {
  if (!memberStoryFormRef.value || !memberStoryFormRef.value.isValid) return;

  isSaving.value = true;
  try {
    if (editedMemberStory.value) {
      const result = await memberStoryStore.updateItem(editedMemberStory.value);
      if (result.ok) {
        showSnackbar(t('memberStory.edit.saveSuccess'), 'success');
        emit('saved');
      } else {
        showSnackbar(t('memberStory.edit.saveFailed'), 'error');
      }
    }
  } catch (error) {
    console.error('Error saving member story:', error);
    showSnackbar((error as Error).message, 'error');
  } finally {
    isSaving.value = false;
  }
};

const handleClose = () => {
  emit('close');
};

onMounted(() => {
  if (props.memberStoryId) {
    loadMemberStory(props.memberStoryId);
  }
});

watch(() => props.memberStoryId, (newId) => {
  if (newId) {
    loadMemberStory(newId);
  }
});
</script>
