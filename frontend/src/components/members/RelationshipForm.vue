<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submitForm" :disabled="readOnly">
        <v-autocomplete
          v-model="relationshipForm.relatedMemberId"
          :items="availableMembers"
          item-title="fullName"
          item-value="id"
          :label="t('relationship.form.relatedMember')"
          :rules="[rules.required]"
          variant="outlined"
        ></v-autocomplete>
        <v-select
          v-model="relationshipForm.relationshipType"
          :items="relationshipTypeOptions"
          :label="t('relationship.form.relationshipType')"
          :rules="[rules.required]"
          variant="outlined"
        ></v-select>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="$emit('cancel')">{{ readOnly ? t('common.close') : t('common.cancel') }}</v-btn>
      <v-btn v-if="!readOnly" color="blue-darken-1" variant="text" @click="submitForm">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMembers } from '@/data/members';
import type { Member } from '@/types/member';

const props = defineProps<{
  initialRelationshipData?: any; // Define a more specific type later
  readOnly?: boolean;
  title: string;
}>();

const emit = defineEmits(['submit', 'cancel']);

const { t } = useI18n();
const { members: allMembers } = useMembers();

const form = ref<HTMLFormElement | null>(null);

const relationshipForm = ref(props.initialRelationshipData || {
  relatedMemberId: null,
  relationshipType: '',
});

const availableMembers = computed(() => {
  // Filter out the current member from the list of available members for relationships
  // This component doesn't know the current member, so it will just return all members
  return allMembers.value;
});

const relationshipTypeOptions = computed(() => [
  { title: t('relationship.type.parent'), value: 'parent' },
  { title: t('relationship.type.child'), value: 'child' },
  { title: t('relationship.type.spouse'), value: 'spouse' },
  { title: t('relationship.type.other'), value: 'other' },
]);

const rules = {
  required: (value: string) => !!value || t('validation.required'),
};

const submitForm = async () => {
  const { valid } = await form.value!.validate();
  if (valid) {
    emit('submit', relationshipForm.value);
  }
};

watch(() => props.initialRelationshipData, (newVal) => {
  if (newVal) {
    relationshipForm.value = { ...newVal };
  }
}, { immediate: true });
</script>