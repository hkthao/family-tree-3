<template>
  <v-form ref="formRef" @submit.prevent="submitForm" :disabled="props.readOnly">
    <AvatarInput v-if="!props.readOnly" v-model="formData.avatarUrl" :size="96" />
    <div v-else class="d-flex justify-center mb-4">
      <AvatarDisplay :src="formData.avatarUrl" :size="96" />
    </div>

    <v-row>
      <v-col cols="12" md="6" >
        <v-text-field v-model="formData.name" :label="$t('family.form.nameLabel')" @blur="v$.name.$touch()"
          @input="v$.name.$touch()" :error-messages="v$.name.$errors.map(e => e.$message as string)" required
          data-testid="family-name-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
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
        <v-textarea v-model="formData.description" :label="$t('family.form.descriptionLabel')"
          data-testid="family-description-input"></v-textarea>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <UserAutocomplete v-model="managers" chips closable-chips multiple :disabled="props.readOnly"
          :label="t('family.permissions.managers')" data-testid="family-managers-select"></UserAutocomplete>
      </v-col>
      <v-col>
        <UserAutocomplete v-model="viewers" chips closable-chips multiple :disabled="props.readOnly"
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

const props = defineProps<{
  initialFamilyData?: Family;
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
    familyUsers: []
  },
);

watch(
  () => props.initialFamilyData,
  (newVal) => {
    if (newVal) {
      Object.assign(formData, newVal);
      familyUsers.value = newVal.familyUsers || [];
    }
  },
  { deep: true }
);

const rules = useFamilyRules();

const v$ = useVuelidate(rules, formData);

const familyUsers = ref<FamilyUser[]>(props.initialFamilyData?.familyUsers || []);
const Manager = 0;
const Viewer = 1;

const managers = computed({
  get: () => familyUsers.value.filter(fu => fu.role === 0).map(fu => fu.userId),
  set: (newuserIds) => {
    const newManagers = newuserIds.map(id => ({ userId: id, role: Manager }));
    const otherUsers = familyUsers.value.filter(fu => fu.role !== Manager);
    familyUsers.value = [...otherUsers, ...newManagers];
  }
});

const viewers = computed({
  get: () => familyUsers.value.filter(fu => fu.role === Viewer).map(fu => fu.userId),
  set: (newuserIds) => {
    const newViewers = newuserIds.map(id => ({ userId: id, role: Viewer }));
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
  formData.familyUsers = familyUsers.value;
  return formData;
};

defineExpose({
  validate,
  getFormData,
});
</script>
