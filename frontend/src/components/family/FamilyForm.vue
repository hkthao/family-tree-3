<template>
  <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
    <v-col cols="12">
      <AvatarInput
        v-if="!props.readOnly"
        v-model="familyForm.avatarUrl"
        :size="96"
      />
      <div v-else class="d-flex justify-center mb-4">
        <AvatarDisplay :src="familyForm.avatarUrl" :size="96" />
      </div>
    </v-col>
      
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="familyForm.name"
          :label="$t('family.form.nameLabel')"
          :rules="[rules.required]"
          required
          variant="outlined"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="familyForm.visibility"
          :items="visibilityItems"
          :label="$t('family.form.visibilityLabel')"
          required
          variant="outlined"
        ></v-select>
      </v-col>
    </v-row>
    <v-text-field
      v-model="familyForm.address"
      :label="$t('family.form.addressLabel')"
      variant="outlined"
    ></v-text-field>
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
import AvatarInput from '@/components/common/AvatarInput.vue';
import AvatarDisplay from '@/components/common/AvatarDisplay.vue';

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
