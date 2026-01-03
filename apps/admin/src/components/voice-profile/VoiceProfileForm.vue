<template>
  <v-form ref="form" >
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="editableVoiceProfile.label" :label="t('voiceProfile.form.name')"
          :rules="voiceProfileRules.label" :readonly="readOnly" data-testid="voice-profile-name"></v-text-field>
      </v-col>
      <v-col cols="12">
        <MediaInput
          v-model="editableVoiceProfile.rawAudioUrls"
          :label="t('voiceProfile.form.audioUrls')"
          :family-id="familyId"
          selection-mode="multiple"
          :initial-media-type="MediaType.Audio"
          :rules="voiceProfileRules.rawAudioUrls"
          data-testid="voice-profile-audio-urls"
          :disabled="readOnly"
          :allow-upload="true"
        ></MediaInput>
      </v-col>
      <v-col cols="6">
        <v-text-field v-model.number="editableVoiceProfile.durationSeconds"
          :label="t('voiceProfile.form.durationSeconds')" :rules="voiceProfileRules.durationSeconds"
          :readonly="readOnly" type="number" data-testid="voice-profile-duration" disabled></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-select v-model="editableVoiceProfile.language" :label="t('voiceProfile.form.language')"
          :rules="voiceProfileRules.language" :readonly="readOnly" :items="languageOptions"
          data-testid="voice-profile-language"></v-select>
      </v-col>
      <v-col cols="12">
        <v-checkbox v-model="editableVoiceProfile.consent" :label="t('voiceProfile.form.consent')" :readonly="readOnly"
          :rules="voiceProfileRules.consent"
          data-testid="voice-profile-consent"></v-checkbox>
      </v-col>
      <v-col cols="12" v-if="!readOnly && initialVoiceProfileData.id">
        <v-select v-model="editableVoiceProfile.status" :items="voiceProfileStatusOptions"
          :label="t('voiceProfile.form.status')" :rules="voiceProfileRules.label"
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
import { useVoiceProfileValidation } from '@/validations/voice-profile.validation';

export interface IVoiceProfileFormInstance {
  validate: () => Promise<{ valid: boolean; errors?: any[] }>;
  reset: () => void;
  getData: () => {
    label: string;
    audioUrl: string | null; // Changed to single string
    durationSeconds: number;
    language: string;
    consent: boolean;
    status: VoiceProfileStatus; // Added status
  };
}

const props = defineProps({
  initialVoiceProfileData: {
    type: Object as () => VoiceProfile,
    default: () => ({
      id: '',
      memberId: '',
      label: '',
      audioUrl: '', // Initial audioUrl for existing profile
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
const { voiceProfileRules } = useVoiceProfileValidation();

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
  rawAudioUrls: props.initialVoiceProfileData.audioUrl ? [{
    id: 'existing-' + props.initialVoiceProfileData.audioUrl,
    familyId: props.initialVoiceProfileData.memberId,
    fileName: 'Existing Audio',
    filePath: props.initialVoiceProfileData.audioUrl,
    mediaType: MediaType.Audio,
    fileSize: 0,
    created: new Date(), // Changed to new Date()
    createdBy: 'system',
  }] : [], // Initialize from audioUrl
  durationSeconds: props.initialVoiceProfileData.durationSeconds,
  language: props.initialVoiceProfileData.language,
  consent: props.initialVoiceProfileData.consent,
  status: props.initialVoiceProfileData.status,
});

// Watch for initialVoiceProfileData changes to update editableVoiceProfile
watch(() => props.initialVoiceProfileData, (newData) => {
  // Only update fields that are part of the form, and map audioUrl to rawAudioUrls
  editableVoiceProfile.label = newData.label;
  editableVoiceProfile.rawAudioUrls = newData.audioUrl ? [{
    id: 'existing-' + newData.audioUrl, // Use a unique ID
    familyId: newData.memberId,
    fileName: 'Existing Audio',
    filePath: newData.audioUrl,
    mediaType: MediaType.Audio,
    fileSize: 0, // Default or fetch actual size if available
    created: new Date(), // Changed to new Date()
    createdBy: 'system', // Default or actual user if available
  }] : [];
  editableVoiceProfile.durationSeconds = newData.durationSeconds;
  editableVoiceProfile.language = newData.language;
  editableVoiceProfile.consent = newData.consent;
  editableVoiceProfile.status = newData.status;
}, { deep: true, immediate: true }); // immediate: true to run on initial setup


const voiceProfileStatusOptions = ref([
  { title: t('voiceProfile.status.active'), value: VoiceProfileStatus.Active },
  { title: t('voiceProfile.status.archived'), value: VoiceProfileStatus.Archived },
]);

const languageOptions = ref([
  { title: t('common.language.vi'), value: 'vi' },
  { title: t('common.language.en'), value: 'en' },
]);

onMounted(() => {
  // if (!editableVoiceProfile.id) { // Not needed for a pure add form, initialVoiceProfileData.memberId takes care
  //   editableVoiceProfile.memberId = props.memberId;
  // }
});


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
      rawAudioUrls: props.initialVoiceProfileData.audioUrl ? [{
        id: 'existing-' + props.initialVoiceProfileData.audioUrl,
        familyId: props.initialVoiceProfileData.memberId,
        fileName: 'Existing Audio',
        filePath: props.initialVoiceProfileData.audioUrl,
        mediaType: MediaType.Audio,
        fileSize: 0,
        created: new Date(), // Changed to new Date()
        createdBy: 'system',
      }] : [],
      durationSeconds: props.initialVoiceProfileData.durationSeconds,
      language: props.initialVoiceProfileData.language,
      consent: props.initialVoiceProfileData.consent,
      status: props.initialVoiceProfileData.status,
    });
  },
  getData: () => ({
    label: editableVoiceProfile.label,
    audioUrl: editableVoiceProfile.rawAudioUrls.length > 0 ? editableVoiceProfile.rawAudioUrls[0].filePath : null, // Take first URL
    durationSeconds: editableVoiceProfile.durationSeconds,
    language: editableVoiceProfile.language,
    consent: editableVoiceProfile.consent,
    status: editableVoiceProfile.status,
  }),
});
</script>

<style scoped>
/* Add any specific styles for VoiceProfileForm here */
</style>