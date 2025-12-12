<template>
  <v-form ref="formRef" @submit.prevent="submitForm" :disabled="props.readOnly">
    <AvatarInput v-if="!props.readOnly" v-model="formData.avatarBase64" :size="96"
      :initial-avatar="initialAvatarDisplay" />
    <div v-else class="d-flex justify-center mb-4">
      <AvatarDisplay :src="getFamilyAvatarUrl(formData.avatarUrl)" :size="96" />
    </div>
    <v-row>
      <v-col cols="6">
        <v-text-field v-model="formData.name" :label="$t('family.form.nameLabel')" @blur="v$.name.$touch()"
          @input="v$.name.$touch()" :error-messages="v$.name.$errors.map(e => e.$message as string)" required
          data-testid="family-name-input"></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-select v-model="formData.visibility" :items="visibilityItems" :label="$t('family.form.visibilityLabel')"
          required data-testid="family-visibility-select"></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.address" :label="$t('family.form.addressLabel')"
          data-testid="family-address-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :rows="2" :auto-grow="true"
          :label="$t('family.form.descriptionLabel')" data-testid="family-description-input"></v-textarea>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <UserAutocomplete v-model="managers" multiple :disabled="props.readOnly" hideDetails
          :label="t('family.permissions.managers')" data-testid="family-managers-select"></UserAutocomplete>
      </v-col>
      <v-col cols="12">
        <UserAutocomplete v-model="viewers" multiple :disabled="props.readOnly" hideDetails
          :label="t('family.permissions.viewers')" data-testid="family-viewers-select"></UserAutocomplete>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family, FamilyUser } from '@/types';
import { FamilyVisibility } from '@/types';
import { AvatarInput, AvatarDisplay } from '@/components/common';
import UserAutocomplete from '@/components/common/UserAutocomplete.vue';
import { useVuelidate } from '@vuelidate/core';
import { useFamilyRules } from '@/validations/family.validation';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils'; // NEW

const props = defineProps<{
  data?: Family;
  readOnly?: boolean;
}>();
const emit = defineEmits(['submit', 'cancel']);
const { t } = useI18n();

const formData = reactive<Family | Omit<Family, 'id'>>(
  props.data ? JSON.parse(JSON.stringify(props.data)) : {
    name: '',
    description: '',
    address: '',
    avatarUrl: '',
    avatarBase64: null, // NEW FIELD
    visibility: FamilyVisibility.Public,
    familyUsers: [],
  },
);

// Computed property to pass the initial avatar URL to AvatarInput
const initialAvatarDisplay = computed(() => {
  return formData.avatarBase64 || formData.avatarUrl;
});

watch(
  () => props.data,
  (newVal) => {
    if (newVal) {
      Object.assign(formData, newVal);
      familyUsers.value = newVal.familyUsers || [];
      formData.avatarBase64 = null; // Reset avatarBase64 when initial data changes
    }
  },
  { deep: true }
);

const rules = useFamilyRules();

const v$ = useVuelidate(rules, formData);

const familyUsers = ref<FamilyUser[]>(props.data?.familyUsers || []);
const Manager = 0;
const Viewer = 1;

const managers = computed({
  get: () => familyUsers.value.filter(fu => fu.role === Manager).map(fu => fu.userId),
  set: (newUserIds: string[]) => {
    const newManagers = newUserIds.map(userId => ({ userId: userId, role: Manager }));
    const otherUsers = familyUsers.value.filter(fu => fu.role !== Manager);
    familyUsers.value = [...otherUsers, ...newManagers];
  }
});

const viewers = computed({
  get: () => familyUsers.value.filter(fu => fu.role === Viewer).map(fu => fu.userId),
  set: (newUserIds: string[]) => {
    const newViewers = newUserIds.map(userId => ({ userId: userId, role: Viewer }));
    const otherUsers = familyUsers.value.filter(fu => fu.role !== Viewer);
    familyUsers.value = [...otherUsers, ...newViewers];
  }
});

const visibilityItems = computed(() => [
  {
    title: t('family.form.visibility.private'),
    value: FamilyVisibility.Private,
  },
  { title: t('family.form.visibility.public'), value: FamilyVisibility.Public },
]);


const submitForm = async () => {
  const result = await v$.value.$validate();
  if (result) {
    emit('submit', formData);
  }
};

// Expose form validation and data for parent component
const validate = async () => {
  const result = await v$.value.$validate();
  return result;
};

const getFormData = () => {
  const dataToSubmit = { ...formData };
  dataToSubmit.familyUsers = familyUsers.value;
  return dataToSubmit;
};

defineExpose({
  validate,
  getFormData,
});
</script>
