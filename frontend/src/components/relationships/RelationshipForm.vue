<template>
  <v-form ref="form" @submit.prevent="save" :disabled="props.readOnly">
    <v-card>
      <v-card-title>{{ formTitle }}</v-card-title>
      <v-card-text>
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
      </v-card-text>
      <v-card-actions v-if="!props.readOnly">
        <v-spacer></v-spacer>
        <v-btn color="primary" type="submit">{{ t('common.save') }}</v-btn>
        <v-btn @click="cancel">{{ t('common.cancel') }}</v-btn>
      </v-card-actions>
    </v-card>
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

const formTitle = computed(() =>
  props.id ? t('relationship.form.editTitle') : t('relationship.form.addTitle'),
);

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS;

const rules = {
  required: (value: any) => !!value || t('validation.required'),
};

onMounted(async () => {
  if (props.id && !props.initialRelationshipData) { // Only fetch if not provided
    await relationshipStore.getById(props.id);
    editableRelationship.value = { ...relationshipStore.currentItem };
  }
});

const validate = async () => { // Added
  if (form.value) {
    const { valid } = await form.value.validate();
    return valid;
  }
  return false;
};

const getFormData = () => { // Added
  return editableRelationship.value;
};

const save = () => {
  emit('save', editableRelationship.value);
};

const cancel = () => {
  emit('cancel');
};

defineExpose({ // Added
  validate,
  getFormData,
});
</script>
