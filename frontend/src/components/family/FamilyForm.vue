<template>
  <v-card>
    <v-card-title>{{ isEditMode ? $t('family.form.editTitle') : $t('family.form.addTitle') }}</v-card-title>
    <v-card-text>
      <v-form ref="form" v-model="isValid">
        <v-text-field
          v-model="familyForm.Name"
          :label="$t('family.form.nameLabel')"
          :rules="[rules.required]"
          required
        ></v-text-field>
        <v-textarea
          v-model="familyForm.Description"
          :label="$t('family.form.descriptionLabel')"
        ></v-textarea>
        <v-text-field
          v-model="familyForm.AvatarUrl"
          :label="$t('family.form.avatarUrlLabel')"
        ></v-text-field>
        <v-select
          v-model="familyForm.Visibility"
          :items="visibilityItems"
          :label="$t('family.form.visibilityLabel')"
          required
        ></v-select>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" text @click="$emit('cancel')">{{ $t('common.cancel') }}</v-btn>
      <v-btn color="primary" text @click="saveFamily" :disabled="!isValid">{{ $t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Family } from '@/data/families';

interface Props {
  family?: Family;
}

const props = defineProps<Props>();
const emit = defineEmits(['save', 'cancel']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);
const isValid = ref(false);

const familyForm = ref<Omit<Family, 'id'> & { id?: number }>({
  Name: '',
  Description: '',
  AvatarUrl: '',
  Visibility: 'Private',
});

const isEditMode = computed(() => !!props.family);

const visibilityItems = computed(() => [
  { title: t('family.form.visibility.private'), value: 'Private' },
  { title: t('family.form.visibility.public'), value: 'Public' },
]);

watch(
  () => props.family,
  (newFamily) => {
    if (newFamily) {
      familyForm.value = { ...newFamily };
    } else {
      familyForm.value = {
        Name: '',
        Description: '',
        AvatarUrl: '',
        Visibility: 'Private',
      };
    }
  },
  { immediate: true }
);

const rules = {
  required: (value: string) => !!value || t('family.form.rules.nameRequired'),
};

const saveFamily = async () => {
  const { valid } = await form.value!.validate();
  if (valid) {
    emit('save', familyForm.value);
  }
};
</script>