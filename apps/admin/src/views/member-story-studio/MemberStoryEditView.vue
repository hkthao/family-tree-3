<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.edit.title') }}</span>
    </v-card-title>
    <MemberStoryForm
      v-if="editedMemberStory"
      ref="memberStoryFormRef"
      v-model="editedMemberStory"
      :member-id="editedMemberStory.memberId"

      :readonly="false"
    />
    <v-card-text v-else>
      <v-alert type="info">{{ t('memberStory.edit.loading') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="handleClose">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving">
        {{ t('common.save') }}
      </v-btn>

    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory';
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue';

const { memberStoryId } = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const { showSnackbar } = useGlobalSnackbar();

const memberStoryFormRef = ref<InstanceType<typeof MemberStoryForm> | null>(null);
const editedMemberStory = ref<MemberStoryDto | null>(null);

const isSaving = ref(false);

onMounted(async () => {
  const story = await memberStoryStore.getById(memberStoryId);
  if (story) {
    editedMemberStory.value = story;
  }
});

const handleSave = async () => {
  if (!editedMemberStory.value || !memberStoryFormRef.value) return; // Updated

  isSaving.value = true;
  try {
    const result = await memberStoryStore.updateItem(editedMemberStory.value); // Updated
    if (result.ok) {
      showSnackbar(t('memberStory.edit.saveSuccess'), 'success'); // Updated
      emit('saved');
    } else {
      showSnackbar(t('memberStory.edit.saveFailed'), 'error'); // Updated
    }
  } catch (error) {
    console.error('Error saving member story:', error); // Updated
    // Corrected showSnackbar call: message as first arg, color as second
    showSnackbar((error as Error).message, 'error');
  } finally {
    isSaving.value = false;
  }
};

const handleClose = () => {
  emit('close');
};

</script>