<template>
  <v-card>
    <v-card-title>{{ isEditMode ? 'Chỉnh sửa Family' : 'Thêm mới Family' }}</v-card-title>
    <v-card-text>
      <v-form ref="form" v-model="isValid">
        <v-text-field
          v-model="familyForm.Name"
          label="Name"
          :rules="[rules.required]"
          required
        ></v-text-field>
        <v-textarea
          v-model="familyForm.Description"
          label="Description"
        ></v-textarea>
        <v-text-field
          v-model="familyForm.AvatarUrl"
          label="Avatar URL"
        ></v-text-field>
        <v-select
          v-model="familyForm.Visibility"
          :items="['Private', 'Public']"
          label="Visibility"
          required
        ></v-select>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" text @click="$emit('cancel')">Hủy</v-btn>
      <v-btn color="primary" text @click="saveFamily" :disabled="!isValid">Lưu</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type { Family } from '@/data/families';

interface Props {
  family?: Family;
}

const props = defineProps<Props>();
const emit = defineEmits(['save', 'cancel']);

const form = ref<HTMLFormElement | null>(null);
const isValid = ref(false);

const familyForm = ref<Omit<Family, 'id'> & { id?: number }>({
  Name: '',
  Description: '',
  AvatarUrl: '',
  Visibility: 'Private',
});

const isEditMode = computed(() => !!props.family);

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
  required: (value: string) => !!value || 'Name is required.',
};

const saveFamily = async () => {
  const { valid } = await form.value!.validate();
  if (valid) {
    emit('save', familyForm.value);
  }
};
</script>
