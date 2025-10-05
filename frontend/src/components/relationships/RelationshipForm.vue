<template>
  <v-form ref="form" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12" md="6">
        <MemberAutocomplete
          v-model="editableRelationship.sourceMemberId"
          :label="t('relationship.form.sourceMember')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        />
      </v-col>
      <v-col cols="12" md="6">
        <MemberAutocomplete
          v-model="editableRelationship.targetMemberId"
          :label="t('relationship.form.targetMember')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        />
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <v-select
          v-model="editableRelationship.type"
          :items="relationshipTypes"
          :label="t('relationship.form.type')"
          :rules="[rules.required]"
          :readonly="props.readOnly"
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model.number="editableRelationship.order"
          :label="t('relationship.form.order')"
          type="number"
          :readonly="props.readOnly"
        ></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';

import { useRelationshipStore } from '@/stores/relationship.store';

import type { Relationship } from '@/types';
import { RELATIONSHIP_TYPE_OPTIONS } from '@/constants/relationshipTypes';

import { MemberAutocomplete } from '@/components/common';

const props = defineProps<{
  id?: string; // Added
  readOnly?: boolean;
  initialRelationshipData?: Relationship;
}>();
const emit = defineEmits(['save', 'cancel']);

const { t } = useI18n();
const relationshipStore = useRelationshipStore();

const form = ref<HTMLFormElement | null>(null); // Added

const editableRelationship = ref<Partial<Relationship>>(
  props.initialRelationshipData
    ? { ...props.initialRelationshipData }
    : {
        sourceMemberId: '',
        targetMemberId: '',
        type: undefined,
        order: undefined,
      },
);

// formTitle is removed

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS;

const rules = {
  required: (value: any) => !!value || t('validation.required'),
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
