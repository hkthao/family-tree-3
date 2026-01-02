<template>
  <v-form ref="form" @submit.prevent="save">
    <v-container>
      <v-row>
        <v-col cols="12" md="6">
          <v-text-field
            v-model="editableVoiceProfile.label"
            :label="t('voiceProfile.form.name')"
            :rules="[rules.required]"
            :readonly="readOnly"
            data-testid="voice-profile-name"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            v-model="editableVoiceProfile.audioUrl"
            :label="t('voiceProfile.form.audioUrl')"
            :rules="[rules.required, rules.url]"
            :readonly="readOnly"
            data-testid="voice-profile-audio-url"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            v-model.number="editableVoiceProfile.durationSeconds"
            :label="t('voiceProfile.form.durationSeconds')"
            :rules="[rules.required, rules.positiveNumber]"
            :readonly="readOnly"
            type="number"
            data-testid="voice-profile-duration"
          ></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            v-model="editableVoiceProfile.language"
            :label="t('voiceProfile.form.language')"
            :rules="[rules.required]"
            :readonly="readOnly"
            data-testid="voice-profile-language"
          ></v-text-field>
        </v-col>
        <v-col cols="12">
          <v-checkbox
            v-model="editableVoiceProfile.consent"
            :label="t('voiceProfile.form.consent')"
            :readonly="readOnly"
            data-testid="voice-profile-consent"
          ></v-checkbox>
        </v-col>
        <v-col cols="12" v-if="!readOnly && initialVoiceProfileData.id">
          <v-select
            v-model="editableVoiceProfile.status"
            :items="voiceProfileStatusOptions"
            :label="t('voiceProfile.form.status')"
            :rules="[rules.required]"
            data-testid="voice-profile-status"
          ></v-select>
        </v-col>
      </v-row>
      <v-row v-if="!readOnly">
        <v-col cols="12">
          <v-btn type="submit" color="primary" class="mr-2">{{ t('common.save') }}</v-btn>
          <v-btn color="grey" @click="cancel">{{ t('common.cancel') }}</v-btn>
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, reactive, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VoiceProfile } from '@/types';
import { VoiceProfileStatus } from '@/types';

export interface IVoiceProfileFormInstance {
  validate: () => Promise<{ valid: boolean; errors?: any[] }>;
  reset: () => void;
  getData: () => VoiceProfile;
}

const props = defineProps({
  initialVoiceProfileData: {
    type: Object as () => VoiceProfile,
    default: () => ({
      id: '',
      memberId: '',
      label: '',
      audioUrl: '',
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
  readOnly: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['save', 'cancel']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);
const editableVoiceProfile = reactive<VoiceProfile>({ ...props.initialVoiceProfileData });

const voiceProfileStatusOptions = ref([
  { title: t('voiceProfile.status.active'), value: VoiceProfileStatus.Active },
  { title: t('voiceProfile.status.archived'), value: VoiceProfileStatus.Archived },
]);

watch(() => props.initialVoiceProfileData, (newData) => {
  Object.assign(editableVoiceProfile, newData);
}, { deep: true });

onMounted(() => {
  if (!editableVoiceProfile.id) {
    editableVoiceProfile.memberId = props.memberId;
  }
});

const rules = {
  required: (value: string) => !!value || t('common.validations.required'),
  url: (value: string) => !!value && /^(ftp|http|https):\/\/[^ "]+$/.test(value) || t('common.validations.url'),
  positiveNumber: (value: number) => value > 0 || t('common.validations.positiveNumber'),
};

const save = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    if (valid) {
      emit('save', editableVoiceProfile);
    }
  }
};

const cancel = () => {
  emit('cancel');
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
    Object.assign(editableVoiceProfile, props.initialVoiceProfileData);
  },
  getData: () => editableVoiceProfile,
});
</script>

<style scoped></style>
