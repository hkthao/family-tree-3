<template>
  <v-form ref="form" v-model="valid" lazy-validation data-testid="user-push-token-form">
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.userId" :label="t('userPushToken.form.userId')" :rules="userIdRules" required
          :readonly="readOnly || isEditing" data-testid="user-push-token-form-userId"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.expoPushToken" :label="t('userPushToken.form.expoPushToken')"
          :rules="expoPushTokenRules" required :readonly="readOnly"
          data-testid="user-push-token-form-expoPushToken"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.platform" :label="t('userPushToken.form.platform')" :rules="platformRules"
          required :readonly="readOnly" data-testid="user-push-token-form-platform"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.deviceId" :label="t('userPushToken.form.deviceId')" :rules="deviceIdRules"
          required :readonly="readOnly || isEditing" data-testid="user-push-token-form-deviceId"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-checkbox v-model="formData.isActive" :label="t('userPushToken.form.isActive')" :readonly="readOnly"
          data-testid="user-push-token-form-isActive"></v-checkbox>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type {
  CreateUserPushTokenCommand,
  UpdateUserPushTokenCommand,
  UserPushTokenDto,
} from '@/types';

interface Props {
  initialData?: UserPushTokenDto;
  readOnly?: boolean;
  isEditing?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  readOnly: false,
  isEditing: false,
});

const emit = defineEmits(['update:isValid']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);
const valid = ref(true);

const formData = reactive<CreateUserPushTokenCommand | UpdateUserPushTokenCommand>(
  props.initialData
    ? { ...props.initialData }
    : {
      userId: '',
      expoPushToken: '',
      platform: '',
      deviceId: '',
      isActive: true,
    },
);

const userIdRules = [
  (v: string) => !!v || t('userPushToken.form.validation.userIdRequired'),
  (v: string) =>
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(v) ||
    t('userPushToken.form.validation.userIdInvalid'),
];
const expoPushTokenRules = [
  (v: string) => !!v || t('userPushToken.form.validation.expoPushTokenRequired'),
  (v: string) =>
    (v && v.length <= 500) || t('userPushToken.form.validation.expoPushTokenLength'),
];
const platformRules = [
  (v: string) => !!v || t('userPushToken.form.validation.platformRequired'),
  (v: string) =>
    (v && v.length <= 50) || t('userPushToken.form.validation.platformLength'),
];
const deviceIdRules = [
  (v: string) => !!v || t('userPushToken.form.validation.deviceIdRequired'),
  (v: string) =>
    (v && v.length <= 200) || t('userPushToken.form.validation.deviceIdLength'),
];

watch(valid, (newValue) => {
  emit('update:isValid', newValue);
});

async function validate(): Promise<boolean> {
  const { valid } = await form.value!.validate();
  return valid;
}

function getFormData(): CreateUserPushTokenCommand | UpdateUserPushTokenCommand {
  return { ...formData };
}

defineExpose({
  validate,
  getFormData,
});

// For component interface definition
export interface IUserPushTokenFormInstance {
  validate: () => Promise<boolean>;
  getFormData: () => CreateUserPushTokenCommand | UpdateUserPushTokenCommand;
}
</script>

<style scoped></style>
