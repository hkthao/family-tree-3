<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
        <v-col cols="12">
          <div class="d-flex justify-center mb-4">
            <v-avatar size="96">
              <v-img v-if="familyForm.avatarUrl" :src="familyForm.avatarUrl"></v-img>
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