<template>
  <v-form ref="form" :disabled="props.readOnly" data-testid="relationship-form">
    <v-row>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="editableRelationship.sourceMemberId" :label="t('relationship.form.sourceMember')"
          :rules="[rules.required]" :readonly="props.readOnly" data-testid="relationship-source-member-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <MemberAutocomplete v-model="editableRelationship.targetMemberId" :label="t('relationship.form.targetMember')"
          :rules="[rules.required]" :readonly="props.readOnly" data-testid="relationship-target-member-autocomplete" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-select v-model="editableRelationship.type" :items="relationshipTypes" :label="t('relationship.form.type')"
          :rules="[rules.required]" :readonly="props.readOnly" data-testid="relationship-type-select"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <FamilyAutocomplete v-model="editableRelationship.familyId" :label="t('relationship.form.family')"
          :rules="[rules.required]" :readonly="props.readOnly" data-testid="relationship-family-autocomplete" />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model.number="editableRelationship.order" :label="t('relationship.form.order')" type="number"
          :readonly="props.readOnly" data-testid="relationship-order-input"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';

import { useRelationshipStore } from '@/stores/relationship.store';

import type { Relationship } from '@/types';
import { RELATIONSHIP_TYPE_OPTIONS } from '@/constants/relationshipTypes';

import { MemberAutocomplete, FamilyAutocomplete } from '@/components/common';

const props = defineProps<{
  id?: string;
  readOnly?: boolean;
  initialRelationshipData?: Relationship;
}>();


const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const form = ref<HTMLFormElement | null>(null);

const editableRelationship = ref<Partial<Relationship>>(
  props.initialRelationshipData
    ? { ...props.initialRelationshipData }
    : {
      sourceMemberId: '',
      targetMemberId: '',
      type: undefined,
      order: undefined,
      familyId: undefined
    },
);

// formTitle is removed

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS;

const rules = {
  required: (value: any) => (value !== null && value !== undefined && value !== '') || t('validation.required'),
};

onMounted(async () => {
  if (props.id && !props.initialRelationshipData) {
    await relationshipStore.getById(props.id);
    editableRelationship.value = { ...relationshipStore.currentItem };
  }
});

const validate = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    return valid;
  }
  return false;
};

const getFormData = () => {
  return editableRelationship.value;
};

// save and cancel methods are removed

defineExpose({
  validate,
  getFormData,
});
</script>
