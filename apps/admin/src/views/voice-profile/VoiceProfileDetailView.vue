<template>
  <v-card :elevation="0" data-testid="voice-profile-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('voiceProfile.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert type="error" :text="error?.message || t('voiceProfile.detail.errorLoading')"></v-alert>
      </div>
      <div v-else-if="voiceProfile">
        <!-- PrivacyAlert if needed, based on backend DTO -->
        <VoiceProfileForm :initial-voice-profile-data="voiceProfile" :member-id="props.memberId" :family-id="props.familyId" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import VoiceProfileForm from '@/components/voice-profile/VoiceProfileForm.vue';
import { useVoiceProfileDetail } from '@/composables/voice-profile/useVoiceProfileDetail';
// import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // TODO: Check if needed

const props = defineProps({
  memberId: {
    type: String as PropType<string>,
    required: true,
  },
  voiceProfileId: {
    type: String as PropType<string>,
    required: true,
  },
  familyId: { // New prop
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const { state: { voiceProfile, isLoading, error }, actions: { closeView } } = useVoiceProfileDetail({
  memberId: ref(props.memberId),
  voiceProfileId: ref(props.voiceProfileId),
  onClose: () => {
    emit('close');
  },
});
</script>

<style scoped></style>
