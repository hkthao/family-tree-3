<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.edit.title') }}</span>
    </v-card-title>
    <MemberStoryForm
      v-if="editedMemberStory"
      ref="memberStoryFormRef"
      :model-value="editedMemberStory"
      @update:modelValue="handleMemberStoryFormUpdate"
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
import type { DetectedFace } from '@/types'; // Added this import
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
  console.log('MemberStoryEditView mounted, memberStoryId:', memberStoryId);
  const story = await memberStoryStore.getById(memberStoryId);
  if (story) {
    editedMemberStory.value = story;
    // Ensure faces is an array
    if (!editedMemberStory.value.faces) {
      editedMemberStory.value.faces = [];
    }
    console.log('editedMemberStory on mount:', editedMemberStory.value);
    console.log('editedMemberStory.faces on mount:', editedMemberStory.value?.faces?.map((f: DetectedFace) => f.id));
  }
});

const handleMemberStoryFormUpdate = (newValue: MemberStoryDto) => {
  console.log('MemberStoryForm emitted update:modelValue with newValue:', newValue);
  console.log('Faces in emitted newValue:', newValue.faces?.map(f => f.id));
  editedMemberStory.value = newValue;
  console.log('editedMemberStory after update:', editedMemberStory.value);
  console.log('editedMemberStory.faces after update:', editedMemberStory.value?.faces?.map((f: DetectedFace) => f.id));
};

const handleSave = async () => {
  if (!editedMemberStory.value || !memberStoryFormRef.value) return;

  isSaving.value = true;
  console.log('Saving editedMemberStory:', editedMemberStory.value);
  console.log('Faces in editedMemberStory before save:', editedMemberStory.value?.faces?.map((f: DetectedFace) => f.id));
  try {
    const result = await memberStoryStore.updateItem(editedMemberStory.value);
    if (result.ok) {
      showSnackbar(t('memberStory.edit.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(t('memberStory.edit.saveFailed'), 'error');
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

</script>