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
import { ref, watch, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStoryQuery, useUpdateMemberStoryMutation } from '@/composables/memberStory';
import { useGlobalSnackbar } from '@/composables';
import type { MemberStoryDto } from '@/types/memberStory';
import MemberStoryForm from '@/components/member-story/MemberStoryForm.vue'; // Updated import

const props = defineProps<{
  memberStoryId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { data: fetchedMemberStory, isLoading: isQueryLoading, isError: isQueryError, error: queryError } = useMemberStoryQuery(toRef(props, 'memberStoryId'));
const { mutateAsync: updateMemberStory, isPending: isMutationLoading, isError: isMutationError, error: mutationError } = useUpdateMemberStoryMutation();

const memberStoryFormRef = ref<InstanceType<typeof MemberStoryForm> | null>(null);
const editedMemberStory = ref<MemberStoryDto | null>(null);

const loading = computed(() => isQueryLoading.value || isMutationLoading.value);
const isSaving = computed(() => isMutationLoading.value);

watch(fetchedMemberStory, (newValue) => {
  if (newValue) {
    editedMemberStory.value = { ...newValue };
  } else {
    editedMemberStory.value = null;
  }
}, { immediate: true });

watch([isQueryError, fetchedMemberStory], () => {
  if (isQueryError.value) {
    showSnackbar(queryError.value?.message || t('memberStory.edit.loadError'), 'error');
  } else if (!isQueryLoading.value && !fetchedMemberStory.value) {
    showSnackbar(t('memberStory.edit.notFound'), 'error');
  }
}, { immediate: true });

watch(isMutationError, () => {
  if (isMutationError.value) {
    showSnackbar(mutationError.value?.message || t('memberStory.edit.saveFailed'), 'error');
  }
});

const handleSave = async () => {
  if (!memberStoryFormRef.value || !memberStoryFormRef.value.isValid || !editedMemberStory.value) return;

  try {
    await updateMemberStory(editedMemberStory.value);
    showSnackbar(t('memberStory.edit.saveSuccess'), 'success');
    emit('saved');
  } catch (error) {
    console.error('Error saving member story:', error);
    showSnackbar((error as Error).message, 'error');
  }
};

const handleClose = () => {
  emit('close');
};


</script>
