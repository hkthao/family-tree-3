<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.create.title') }}</span>
    </v-card-title>
    {{ editedMemberStory.faces?.map((e: DetectedFace)=>e.id) }}
    <MemberStoryForm
      ref="memberStoryFormRef"
      v-model="editedMemberStory"
      :member-id="memberId"
      :readonly="false"
    />
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
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryStore } from '@/stores/memberStory.store';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';
import type { MemberStoryDto } from '@/types/memberStory';
import type { DetectedFace } from '@/types'; // Added this import
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

const editedMemberStory = ref<MemberStoryDto>({
  memberId: props.memberId || '', // Pre-fill if memberId is provided
  title: '',
  story: '',
  photoUrl: undefined,
  faces: [], // Initialize faces as an empty array
});

const handleSave = async () => {
  if (!memberStoryFormRef.value) return; // Updated

  isSaving.value = true;
  try {
    const result = await memberStoryStore.addItem(editedMemberStory.value); // Updated
    if (result.ok) {
      showSnackbar(t('memberStory.create.step5.saveSuccess'), 'success'); // Updated
      emit('saved');
    } else {
      showSnackbar(t('memberStory.create.step5.saveFailed'), 'error'); // Updated
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