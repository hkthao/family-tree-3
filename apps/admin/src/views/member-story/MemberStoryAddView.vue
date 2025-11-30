<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.create.title') }}</span>
    </v-card-title>
    <MemberStoryForm ref="memberStoryFormRef" v-model="editedMemberStory" :member-id="memberId" :readonly="false" />
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="handleClose">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn color="blue-darken-1" variant="text" @click="handleSave" :loading="isSaving"
        :disabled="!memberStoryFormRef?.isValid">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory'; // Keep MemberStoryDto for local state
import type { CreateMemberStory } from '@/types/createMemberStory'; // New import
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue';

const props = defineProps<{
  memberId?: string; // Optional memberId for pre-filling
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const { showSnackbar } = useGlobalSnackbar();
const memberStoryFormRef = ref<InstanceType<typeof MemberStoryForm> | null>(null); // Updated
const isSaving = ref(false); // To manage loading state for buttons
const editedMemberStory = ref<MemberStoryDto>({ // Keep as MemberStoryDto for form compatibility
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  story: '',
  detectedFaces: [], // Initialize detectedFaces
  originalImageUrl: '',
  resizedImageUrl: '',
  rawInput: null, // Keep for AI generation
  storyStyle: null, // NEW
  perspective: null, // NEW
});

const handleSave = async () => {
  if (!memberStoryFormRef.value || !editedMemberStory.value.memberId || !editedMemberStory.value.title || !editedMemberStory.value.story) return;

  isSaving.value = true;
  try {
    // Map MemberStoryDto to CreateMemberStory for the backend call
    const createPayload: CreateMemberStory = {
      memberId: editedMemberStory.value.memberId,
      title: editedMemberStory.value.title,
      story: editedMemberStory.value.story,
      originalImageUrl: editedMemberStory.value.originalImageUrl,
      resizedImageUrl: editedMemberStory.value.resizedImageUrl,
      rawInput: editedMemberStory.value.rawInput, // NEW
      storyStyle: editedMemberStory.value.storyStyle, // NEW
      perspective: editedMemberStory.value.perspective, // NEW
      detectedFaces: (editedMemberStory.value.detectedFaces || [])
    };

    const result = await memberStoryStore.addItem(createPayload); // Pass the mapped payload
    if (result.ok) {
      showSnackbar(t('memberStory.create.saveSuccess'), 'success'); // Updated
      emit('saved');
    } else {
      showSnackbar(t('memberStory.create.saveFailed'), 'error'); // Updated
    }
  } catch (error) {
    console.error('Error saving member story:', error); // Updated
    showSnackbar((error as Error).message, 'error');
  } finally {
    isSaving.value = false;
  }
};

const handleClose = () => {
  emit('close');
};

</script>