<template>
  <v-form ref="formRef" @submit.prevent="submitForm" :disabled="props.readOnly">
    <AvatarInput v-if="!props.readOnly" v-model="formData.avatarUrl" :size="96" />
    <div v-else class="d-flex justify-center mb-4">
      <AvatarDisplay :src="formData.avatarUrl" :size="96" />
    </div>

    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.name" :label="$t('family.form.nameLabel')" 
          @blur="v$.name.$touch()" @input="v$.name.$touch()"
          :error-messages="v$.name.$errors.map(e => e.$message as string)"
          required data-testid="family-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.visibility" :items="visibilityItems" :label="$t('family.form.visibilityLabel')"
          required data-testid="family-visibility-select"></v-select>
      </v-col>
    </v-row>
    <v-text-field v-model="formData.address" :label="$t('family.form.addressLabel')" data-testid="family-address-input"></v-text-field>
    <v-textarea v-model="formData.description" :label="$t('family.form.descriptionLabel')" data-testid="family-description-input"></v-textarea>
    <FamilyPermissions :readOnly="props.readOnly" v-model="familyUsers" />
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, reactive, toRefs } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family, FamilyUser } from '@/types';
import { FamilyVisibility } from '@/types';
import { AvatarInput, AvatarDisplay } from '@/components/common';
import FamilyPermissions from './FamilyPermissions.vue';
import { useVuelidate } from '@vuelidate/core';
import { useFamilyRules } from '@/validations/family.validation';

const props = defineProps<{
  initialFamilyData?: Family;
  initialFamilyUsers?: FamilyUser[];
  readOnly?: boolean;
}>();
const emit = defineEmits(['submit', 'cancel']);
const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const formData = reactive<Family | Omit<Family, 'id'>>(
  props.initialFamilyData || {
    name: '',
    description: '',
    address: '',
    avatarUrl: '',
    visibility: FamilyVisibility.Public,
  },
);

const state = reactive({
  name: formData.name,
});

const rules = useFamilyRules();

const v$ = useVuelidate(rules, state);

const familyUsers = ref<FamilyUser[]>(props.initialFamilyUsers || []);
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
  return formData;
};

defineExpose({
  validate,
  getFormData,
});
</script>
