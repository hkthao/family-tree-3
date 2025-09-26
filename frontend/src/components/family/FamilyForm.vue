<template>
  <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
    <v-col cols="12">
      <div class="d-flex justify-center mb-4">
        <v-avatar size="96">
          <v-img
            v-if="familyForm.avatarUrl"
            :src="familyForm.avatarUrl"
          ></v-img>
          <v-icon v-else size="96">mdi-account-group</v-icon>
        </v-avatar>
      </div>
    </v-col>
    <v-text-field
      v-model="familyForm.avatarUrl"
      :label="$t('family.form.avatarUrlLabel')"
      variant="outlined"
    ></v-text-field>
    <v-text-field
      v-model="familyForm.name"
      :label="$t('family.form.nameLabel')"
      :rules="[rules.required]"
      required
      variant="outlined"
    ></v-text-field>
    <v-text-field
      v-model="familyForm.address"
      :label="$t('family.form.addressLabel')"
      variant="outlined"
    ></v-text-field>
    <v-select
      v-model="familyForm.visibility"
      :items="visibilityItems"
      :label="$t('family.form.visibilityLabel')"
      required
      variant="outlined"
    ></v-select>
    <v-textarea
      v-model="familyForm.description"
      :label="$t('family.form.descriptionLabel')"
      variant="outlined"
    ></v-textarea>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types/family';
import { FamilyVisibility } from '@/types/family/family-visibility';

const props = defineProps<{
  initialFamilyData?: Family;
  readOnly?: boolean;
}>();
const emit = defineEmits(['submit', 'cancel']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const familyForm = ref<Family | Omit<Family, 'id'>>(
  props.initialFamilyData || {
    name: '',
    description: '',
    address: '',
    avatarUrl: '',
    visibility: FamilyVisibility.Public,
  },
);

const visibilityItems = computed(() => [
  {
    title: t('family.form.visibility.private'),
    value: FamilyVisibility.Private,
  },
  { title: t('family.form.visibility.public'), value: FamilyVisibility.Public },
]);

const rules = {
  required: (value: string) => !!value || t('family.form.rules.nameRequired'),
};

const submitForm = async () => {
  const { valid } = await form.value!.validate();
  if (valid) {
    emit('submit', familyForm.value);
  }
};

// Expose form validation and data for parent component
const validate = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    return valid;
  }
  return false;
};

const getFormData = () => {
  return familyForm.value;
};

defineExpose({
  validate,
  getFormData,
});
</script>
