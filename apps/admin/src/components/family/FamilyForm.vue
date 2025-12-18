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
import { ref } from 'vue';
import type { Family } from '@/types';
import { AvatarInput, AvatarDisplay } from '@/components/common';
import UserAutocomplete from '@/components/common/UserAutocomplete.vue';
import { useFamilyForm } from '@/composables';

const props = defineProps<{
  data?: Family;
  readOnly?: boolean;
}>();
const emit = defineEmits(['submit', 'cancel']);

const formRef = ref<HTMLFormElement | null>(null);

const {
  t,
  formData,
  initialAvatarDisplay,
  v$,
  managers,
  viewers,
  visibilityItems,
  submitForm,
  validate,
  getFormData,
  getFamilyAvatarUrl,
} = useFamilyForm(props, emit);

defineExpose({
  validate,
  getFormData,
});
</script>
