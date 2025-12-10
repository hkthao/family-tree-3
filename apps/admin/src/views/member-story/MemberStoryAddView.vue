<template>
  <v-card flat>
    <v-card-title class="text-center">
      <span class="text-h6">{{ t('memberStory.create.title') }}</span>
    </v-card-title>
    <MemberStoryForm ref="memberStoryFormRef" v-model="editedMemberStory" :readonly="false" :family-id="familyId" />
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
import type { MemberStoryDto } from '@/types/memberStory';

import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue';
import { LifeStage } from '@/types/enums';

const props = defineProps<{
  memberId?: string; // Optional memberId for pre-filling
  familyId: string; // familyId is now required for this view
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const memberStoryStore = useMemberStoryStore();
const { showSnackbar } = useGlobalSnackbar();
const memberStoryFormRef = ref<InstanceType<typeof MemberStoryForm> | null>(null);
const isSaving = ref(false);
const editedMemberStory = ref<MemberStoryDto>({
  memberId: props.memberId || '',
  familyId: props.familyId, // Initialize with prop familyId
  title: '',
  story: '',
  year: null,
  timeRangeDescription: null,
  lifeStage: LifeStage.Childhood, // Default value
  location: null,
  storytellerId: null,
  detectedFaces: [],

});

const handleSave = async () => {
  if (!memberStoryFormRef.value || !memberStoryFormRef.value.isValid) {
    showSnackbar(t('common.validations.required'), 'error');
    return;
  }

  isSaving.value = true;
  try {
    const createPayload: Omit<MemberStoryDto, 'id'> = {
      memberId: editedMemberStory.value.memberId,
      title: editedMemberStory.value.title,
      story: editedMemberStory.value.story,
      year: editedMemberStory.value.year,
      timeRangeDescription: editedMemberStory.value.timeRangeDescription,
      lifeStage: editedMemberStory.value.lifeStage,
      location: editedMemberStory.value.location,
      storytellerId: editedMemberStory.value.storytellerId,
      detectedFaces: editedMemberStory.value.detectedFaces || [],
      temporaryOriginalImageUrl: editedMemberStory.value.temporaryOriginalImageUrl,
      temporaryResizedImageUrl: editedMemberStory.value.temporaryResizedImageUrl,
    };

    const result = await memberStoryStore.addItem(createPayload);
    if (result.ok) {
      showSnackbar(t('memberStory.create.saveSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(result.error?.message || t('common.saveError'), 'error');
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