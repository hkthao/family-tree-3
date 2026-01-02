<template>
  <v-card :elevation="0" data-testid="voice-profile-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('voiceProfile.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <VoiceProfileForm
        ref="voiceProfileFormRef"
        v-if="voiceProfile"
        :initial-voice-profile-data="voiceProfile"
        :member-id="props.memberId"
        :read-only="false"
        @save="handleUpdateItem"
        @cancel="closeForm"
      />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="grey"
        data-testid="button-cancel"
        @click="closeForm"
        :disabled="isLoading || isUpdatingVoiceProfile || isUploadingMedia"
      >{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingVoiceProfile || isUploadingMedia"
        :disabled="isLoading || isUpdatingVoiceProfile || isUploadingMedia"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import VoiceProfileForm from '@/components/voice-profile/VoiceProfileForm.vue';
import { type IVoiceProfileFormInstance } from '@/components/voice-profile/VoiceProfileForm.vue';
import { useVoiceProfileEdit } from '@/composables/voice-profile/useVoiceProfileEdit';

const props = defineProps({
  memberId: {
    type: String as PropType<string>,
    required: true,
  },
  voiceProfileId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close', 'saved']);

const voiceProfileFormRef: Ref<IVoiceProfileFormInstance | null> = ref(null);

const { t } = useI18n();

const {
  state: { voiceProfile, isLoading, isUpdatingVoiceProfile, isUploadingMedia },
  actions: { handleUpdateItem, closeForm },
} = useVoiceProfileEdit({
  memberId: props.memberId,
  voiceProfileId: props.voiceProfileId,
  onSaveSuccess: () => {
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: voiceProfileFormRef,
});
</script>

<style scoped></style>
