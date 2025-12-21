<template>
  <v-form ref="formRef" :disabled="props.readOnly" data-testid="relationship-form">
    <v-row>
      <v-col cols="12" md="6">
        <family-auto-complete v-model="formData.familyId" :label="t('relationship.form.family')"
          :rules="validationRules.familyId" :readonly="props.readOnly"
          data-testid="relationship-family-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="formData.type" :items="relationshipTypes" :label="t('relationship.form.type')"
          :rules="validationRules.type" :readonly="props.readOnly "
          data-testid="relationship-type-select"></v-select>
      </v-col>

    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <member-auto-complete v-model="formData.sourceMemberId" :label="t('relationship.form.sourceMember')"
          :rules="validationRules.sourceMemberId" :readonly="props.readOnly || !formData.familyId"
          :family-id="formData.familyId" data-testid="relationship-source-member-autocomplete" />
      </v-col>
      <v-col cols="12" md="6">
        <member-auto-complete v-model="formData.targetMemberId" :label="t('relationship.form.targetMember')"
          :rules="validationRules.targetMemberId" :readonly="props.readOnly || !formData.familyId"
          :family-id="formData.familyId" data-testid="relationship-target-member-autocomplete" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-text-field v-model.number="formData.order" :label="t('relationship.form.order')" type="number"
          :readonly="props.readOnly" data-testid="relationship-order-input"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { RELATIONSHIP_TYPE_OPTIONS } from '@/constants/relationshipTypes';
import { useRelationshipForm } from '@/composables/relationship/useRelationshipForm';
import type { Relationship } from '@/types';

const relationshipTypes = RELATIONSHIP_TYPE_OPTIONS;
const props = defineProps<{
  id?: string;
  readOnly?: boolean;
  initialRelationshipData?: Relationship;
}>();

const { t } = useI18n();

const {
  state: { formRef, formData, validationRules },
  actions: { validate, getFormData },
} = useRelationshipForm({
  id: props.id,
  readOnly: props.readOnly,
  initialRelationshipData: props.initialRelationshipData,
});

defineExpose({
  validate,
  getFormData,
});
</script>
