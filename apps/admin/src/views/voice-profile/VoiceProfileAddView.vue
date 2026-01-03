<template>
  <v-card :elevation="0" data-testid="voice-profile-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('voiceProfile.form.addTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <VoiceProfileForm ref="voiceProfileFormRef" :family-id="familyId" @cancel="closeForm" @save="handleAddItem" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm"
        :disabled="isAddingVoiceProfile || isUploadingMedia">{{
          t('common.cancel')
        }}</v-btn>
      <v-btn color="primary" data-testid="button-save" @click="handleAddItem"
        :loading="isAddingVoiceProfile || isUploadingMedia" :disabled="isAddingVoiceProfile || isUploadingMedia">{{
          t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import VoiceProfileForm from '@/components/voice-profile/VoiceProfileForm.vue';
import { type IVoiceProfileFormInstance } from '@/components/voice-profile/VoiceProfileForm.vue';
import { useVoiceProfileAdd } from '@/composables/voice-profile/useVoiceProfileAdd';

defineProps({
  familyId: { // New prop
    type: String as PropType<string>,
    required: true,
  },
});

const voiceProfileFormRef: Ref<IVoiceProfileFormInstance | null> = ref(null);
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();

const {
  state: { isAddingVoiceProfile, isUploadingMedia },
  actions: { handleAddItem, closeForm },
} = useVoiceProfileAdd({
  onSaveSuccess: () => {
    emit('close');
    emit('saved');
  },
  onCancel: () => {
    emit('close');
  },
  formRef: voiceProfileFormRef,
});
</script>

<style scoped></style>
