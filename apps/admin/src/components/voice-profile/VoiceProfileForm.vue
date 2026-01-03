<template>
  <v-form ref="form" >
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="editableVoiceProfile.label" :label="t('voiceProfile.form.name')"
          :rules="[rules.required]" :readonly="readOnly" data-testid="voice-profile-name"></v-text-field>
      </v-col>
      <v-col cols="12">
        <MediaInput
          v-model="editableVoiceProfile.rawAudioUrls"
          :label="t('voiceProfile.form.audioUrls')"
          :family-id="familyId"
          selection-mode="multiple"
          :initial-media-type="MediaType.Audio"
          :rules="[rules.rawAudioUrlsRequired]"
          data-testid="voice-profile-audio-urls"
          :disabled="readOnly"
        ></MediaInput>
      </v-col>
      <v-col cols="6">
        <v-text-field v-model.number="editableVoiceProfile.durationSeconds"
          :label="t('voiceProfile.form.durationSeconds')" :rules="[rules.required, rules.positiveNumber]"
          :readonly="readOnly" type="number" data-testid="voice-profile-duration"></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field v-model="editableVoiceProfile.language" :label="t('voiceProfile.form.language')"
          :rules="[rules.required]" :readonly="readOnly" data-testid="voice-profile-language"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-checkbox v-model="editableVoiceProfile.consent" :label="t('voiceProfile.form.consent')" :readonly="readOnly"
          data-testid="voice-profile-consent"></v-checkbox>
      </v-col>
      <v-col cols="12" v-if="!readOnly && initialVoiceProfileData.id">
        <v-select v-model="editableVoiceProfile.status" :items="voiceProfileStatusOptions"
          :label="t('voiceProfile.form.status')" :rules="[rules.required]"
          data-testid="voice-profile-status"></v-select>
      </v-col>
    </v-row>

  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, reactive, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VoiceProfile, FamilyMedia } from '@/types'; // Import FamilyMedia
import { VoiceProfileStatus } from '@/types'; // Import VoiceProfileStatus from '@/types'
import { MediaType } from '@/types/enums'; // Import MediaType from '@/types/enums'
import { MediaInput } from '@/components/common'; // Import MediaInput

export interface IVoiceProfileFormInstance {
  validate: () => Promise<{ valid: boolean; errors?: any[] }>;
  reset: () => void;
  getData: () => {
    label: string;
    rawAudioUrls: string[];
    language: string;
    consent: boolean;
  };
}

const props = defineProps({
  initialVoiceProfileData: {
    type: Object as () => VoiceProfile,
    default: () => ({
      id: '',
      memberId: '',
      label: '',
      rawAudioUrls: [], // Changed from audioUrl
      durationSeconds: 0,
      language: 'vi', // Default language
      consent: false,
      status: VoiceProfileStatus.Active,
      created: new Date().toISOString(),
    }),
  },
  memberId: {
    type: String,
    required: true,
  },
  familyId: { // New prop
    type: String,
    required: true,
  },
  readOnly: {
    type: Boolean,
    default: false,
  },
});



const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);
const editableVoiceProfile = reactive<{
  label: string;
  rawAudioUrls: FamilyMedia[]; // Change to FamilyMedia[]
  durationSeconds: number;
  language: string;
  consent: boolean;
  status: VoiceProfileStatus;
  // Other VoiceProfile properties as needed for initialVoiceProfileData, but not directly for getData
}>({
  label: props.initialVoiceProfileData.label,
  rawAudioUrls: [], // Initialize as empty array of FamilyMedia
  durationSeconds: props.initialVoiceProfileData.durationSeconds,
  language: props.initialVoiceProfileData.language,
  consent: props.initialVoiceProfileData.consent,
  status: props.initialVoiceProfileData.status,
});

// Watch for initialVoiceProfileData changes to update editableVoiceProfile
watch(() => props.initialVoiceProfileData, (newData) => {
  // Only update fields that are part of the form, and map audioUrl to rawAudioUrls
  editableVoiceProfile.label = newData.label;
  // If initialVoiceProfileData has audioUrl, convert it to rawAudioUrls (for editing existing profiles)
  editableVoiceProfile.rawAudioUrls = newData.audioUrl ? [{ id: 'existing', filePath: newData.audioUrl, fileName: 'Existing Audio', mediaType: MediaType.Audio, familyId: newData.memberId, size: 0, mimeType: 'audio/*' }] : [];
  editableVoiceProfile.durationSeconds = newData.durationSeconds;
  editableVoiceProfile.language = newData.language;
  editableVoiceProfile.consent = newData.consent;
  editableVoiceProfile.status = newData.status;
}, { deep: true, immediate: true }); // immediate: true to run on initial setup


const voiceProfileStatusOptions = ref([
  { title: t('voiceProfile.status.active'), value: VoiceProfileStatus.Active },
  { title: t('voiceProfile.status.archived'), value: VoiceProfileStatus.Archived },
]);

onMounted(() => {
  // if (!editableVoiceProfile.id) { // Not needed for a pure add form, initialVoiceProfileData.memberId takes care
  //   editableVoiceProfile.memberId = props.memberId;
  // }
});

const rules = {
  required: (value: string) => !!value || t('common.validations.required'),
  // Removed url validation as MediaInput handles it implicitly
  positiveNumber: (value: number) => value > 0 || t('common.validations.positiveNumber'),
  rawAudioUrlsRequired: (value: FamilyMedia[]) => value.length > 0 || t('common.validations.required'),
};



defineExpose<IVoiceProfileFormInstance>({
  validate: async () => {
    if (form.value) {
      const { valid, errors } = await form.value.validate();
      return { valid, errors };
    }
    return { valid: false };
  },
  reset: () => {
    form.value?.reset();
    Object.assign(editableVoiceProfile, {
      label: props.initialVoiceProfileData.label,
      rawAudioUrls: [], // Reset to empty
      durationSeconds: props.initialVoiceProfileData.durationSeconds,
      language: props.initialVoiceProfileData.language,
      consent: props.initialVoiceProfileData.consent,
      status: props.initialVoiceProfileData.status,
    });
  },
  getData: () => ({
    label: editableVoiceProfile.label,
    rawAudioUrls: editableVoiceProfile.rawAudioUrls.map(media => media.filePath), // Extract filePaths
    language: editableVoiceProfile.language,
    consent: editableVoiceProfile.consent,
  }),
});
</script>

<style scoped></style>
