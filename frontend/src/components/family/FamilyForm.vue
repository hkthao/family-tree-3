<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
        <v-text-field
          v-model="familyForm.name"
          :label="$t('family.form.nameLabel')"
          :rules="[rules.required]"
          required
          variant="outlined"
        ></v-text-field>
        <v-textarea
          v-model="familyForm.description"
          :label="$t('family.form.descriptionLabel')"
          variant="outlined"
        ></v-textarea>
        <v-text-field
          v-model="familyForm.avatarUrl"
          :label="$t('family.form.avatarUrlLabel')"
          variant="outlined"
        ></v-text-field>
        <v-select
          v-model="familyForm.visibility"
          :items="visibilityItems"
          :label="$t('family.form.visibilityLabel')"
          required
          variant="outlined"
        ></v-select>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="$emit('cancel')">{{ props.readOnly ? $t('common.close') : $t('common.cancel') }}</v-btn>
      <v-btn v-if="!props.readOnly" color="blue-darken-1" variant="text" @click="submitForm">{{ $t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/types/family';

const props = defineProps<{
  initialFamilyData?: Family;
  readOnly?: boolean;
  title: string;
}>();
const emit = defineEmits(['submit', 'cancel']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const familyForm = ref<Omit<Family, 'id'> & { id?: number }>(props.initialFamilyData || {
  name: '',
  description: '',
  avatarUrl: '',
  visibility: 'Private',
});

const visibilityItems = computed(() => [
  { title: t('family.form.visibility.private'), value: 'Private' },
  { title: t('family.form.visibility.public'), value: 'Public' },
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
</script>